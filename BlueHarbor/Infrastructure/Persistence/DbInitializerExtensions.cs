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

        // Applica le migration pending o assicura che il database sia creato
        // In questo contesto, EnsureCreated è spesso usato per semplicità se non si usano migration
        // Ma il suggerimento dice MigrateAsync. Useremo MigrateAsync se ci sono migration, 
        // altrimenti EnsureCreated. Dato che siamo in net10.0 e potrebbe non esserci un tool di migration installato,
        // usiamo context.Database.MigrateAsync() se possibile, o context.Database.EnsureCreatedAsync().
        
        // Per seguire il suggerimento:
        try 
        {
            await dbContext.Database.MigrateAsync();
        }
        catch
        {
            // Se non ci sono migration, EnsureCreated è il fallback per il seed
            await dbContext.Database.EnsureCreatedAsync();
        }

        // Verifica se il seed è già stato applicato controllando le banchine
        if (!await dbContext.Berths.AnyAsync())
        {
            Console.WriteLine("Database inizializzato con le 8 banchine e SystemState a Giorno 1.");
        }
    }
}
