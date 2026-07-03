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

        try 
        {
            await dbContext.Database.MigrateAsync();
        }
        catch (Exception)
        {
            Console.WriteLine("Database migration failed. Attempting a clean regeneration...");
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.MigrateAsync();
        }

        Console.WriteLine("Database initialized successfully.");
    }
}
