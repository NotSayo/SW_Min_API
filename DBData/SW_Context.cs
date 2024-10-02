using Microsoft.EntityFrameworkCore;

namespace DBData;

public class SwContext : DbContext
{

    public DbSet<SwCharacter> SW_CHARACTERS { get; set; }

    public SwContext(DbContextOptions<SwContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}