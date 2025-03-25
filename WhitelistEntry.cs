using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

[Index(nameof(MacAddress), IsUnique = true)]
public class WhitelistEntry
{
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "VARCHAR(17)")]
    public string MacAddress { get; set; } = "";

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    public class Validator : AbstractValidator<WhitelistEntry>
    {
        public Validator(WhitelistContext db)
        {
            RuleFor(e => e.MacAddress)
                .NotEmpty()
                .Matches(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$")
                .WithMessage("Invalid MAC address format")
                .Must((entry, mac) => !db.WhitelistEntries.Any(e => e.MacAddress == mac && e.Id != entry.Id))
                .WithMessage("MAC address must be unique");

            RuleFor(e => e.ExpiryDate)
                .GreaterThan(DateTime.Now)
                .WithMessage("Expiry date must be in the future");

            RuleFor(e => e.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Invalid email format");
        }
    }
}
