using Microsoft.EntityFrameworkCore;

public class WhitelistContext : DbContext
{
    public WhitelistContext(DbContextOptions<WhitelistContext> options) : base(options) { }

    public DbSet<WhitelistEntry> WhitelistEntries { get; set; }
}
