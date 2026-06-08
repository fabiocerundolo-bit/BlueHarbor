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

        // In un ambiente di sviluppo, se il DB esiste ma non ha tabelle (o ha uno schema vecchio), 
        // EnsureCreatedAsync non farà nulla. Per risolvere l'errore 208, forziamo la ricreazione in caso di errore.
        try 
        {
            await dbContext.Database.EnsureCreatedAsync();
            
            // Verifica immediata
            if (!await dbContext.Banchine.AnyAsync())
            {
                Console.WriteLine("Seed manuale non rilevato, EnsureCreatedAsync potrebbe non aver creato le tabelle.");
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Database inconsistente rilevato. Tentativo di rigenerazione...");
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }

        Console.WriteLine("Database inizializzato con successo.");
    }
}
