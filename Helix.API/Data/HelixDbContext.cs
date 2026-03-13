using Microsoft.EntityFrameworkCore;

namespace Helix.API.Data;

public class HelixDbContext : DbContext
{

    public HelixDbContext(DbContextOptions<HelixDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<GenomicJob> GenomicJobs { get; set; }
    
}