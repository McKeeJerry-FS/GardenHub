using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace GardenHub.Data
{
    /// <summary>
    /// Design-time factory for ApplicationDbContext.
    /// This allows EF Core tools (migrations, etc.) to create DbContext instances.
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            // Use a dummy connection string for design-time operations
            // This is only used for generating migrations, not runtime
            optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=gardenhub_design;Username=postgres;Password=Momiji25!");
            
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}