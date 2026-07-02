namespace BlueHarbor.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class DbInitializerExtensions
{
    public static async Task InitializeDatabaseAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BlueHarborDbContext>();

        // In a development environment, if the DB exists but has no tables (or has an old schema),
        // EnsureCreatedAsync will do nothing. To resolve error 208, we force recreation on failure.
        try 
        {
            await dbContext.Database.EnsureCreatedAsync();
            
            // Immediate check
            if (!await dbContext.Berths.AnyAsync())
            {
                Console.WriteLine("Manual seed not detected. EnsureCreatedAsync may not have created the tables.");
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Inconsistent database detected. Attempting regeneration...");
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }

        Console.WriteLine("Database initialized successfully.");
    }
}
