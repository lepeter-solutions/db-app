using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<WhitelistContext>(options =>
    options.UseSqlite("Data Source=whitelist.db"));

builder.Services.AddScoped<IValidator<WhitelistEntry>, WhitelistEntry.Validator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/whitelist", (WhitelistContext db) =>
{
    return db.WhitelistEntries;
});

app.MapGet("/whitelist/{id}", (int id, WhitelistContext db) =>
{
    var entry = db.WhitelistEntries.Find(id);
    return entry != null ? Results.Ok(entry) : Results.NotFound(new { Error = "Entry not found" });
});

app.MapPost("/whitelist", async (WhitelistEntry entry, WhitelistContext db, IValidator<WhitelistEntry> validator) =>
{
    var result = await validator.ValidateAsync(entry);
    if (!result.IsValid)
    {
        return Results.ValidationProblem(result.ToDictionary());
    }

    db.WhitelistEntries.Add(entry);
    db.SaveChanges();
    return Results.Ok(entry);
});

app.MapPatch("/whitelist/{id}", async (int id, WhitelistContext db, WhitelistEntry updatedEntry, IValidator<WhitelistEntry> validator) =>
{
    var entry = db.WhitelistEntries.Find(id);
    if (entry == null)
    {
        return Results.NotFound(new { Error = "Entry not found" });
    }

    entry.MacAddress = updatedEntry.MacAddress;
    entry.ExpiryDate = updatedEntry.ExpiryDate;
    entry.Email = updatedEntry.Email;

    var result = await validator.ValidateAsync(entry);
    if (!result.IsValid)
    {
        return Results.ValidationProblem(result.ToDictionary());
    }

    db.SaveChanges();
    return Results.Ok(entry);
});

app.MapDelete("/whitelist/{id}", (int id, WhitelistContext db) =>
{
    var entry = db.WhitelistEntries.Find(id);
    if (entry == null)
    {
        return Results.NotFound(new { Error = "Entry not found" });
    }

    db.WhitelistEntries.Remove(entry);
    db.SaveChanges();
    return Results.NoContent();
});

app.Run();
