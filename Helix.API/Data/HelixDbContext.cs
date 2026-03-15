using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Helix.API.Data;

public class HelixDbContext : IdentityDbContext<ApplicationUser>
{

    public HelixDbContext(DbContextOptions<HelixDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<SmithWatermanAlignmentJob> SmithWatermanAlignmentJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<SmithWatermanAlignmentJob>()
            .HasOne(j => j.User)
            .WithMany(u => u.Jobs)
            .HasForeignKey(j => j.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
}