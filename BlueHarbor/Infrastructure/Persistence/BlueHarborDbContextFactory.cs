using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BlueHarbor.Infrastructure.Persistence
{
    public class BlueHarborDbContextFactory : IDesignTimeDbContextFactory<BlueHarborDbContext>
    {
        public BlueHarborDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BlueHarborDbContext>();

            // Connection string usata SOLO in fase di design (generazione migration)
            optionsBuilder.UseSqlServer(
                "Server=localhost,1433;Database=BlueHarborDb;User Id=sa;Password=BlueHarbor_P@ss1;TrustServerCertificate=True;MultipleActiveResultSets=true");

            return new BlueHarborDbContext(optionsBuilder.Options);
        }
    }
}