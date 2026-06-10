using BlueHarbor.Application.Interfaces;
using BlueHarbor.Infrastructure.Repositories;
using Hangfire;

namespace BlueHarbor.Application.Services;

/// <summary>
/// Servizio responsabile della gestione del tempo virtuale di sistema.
/// Gestisce l'avanzamento dei giorni e innesca i job asincroni per aggiornare lo stato delle navi in transito.
/// </summary>
public class TimeManagementService(
    ISystemStateRepository systemStateRepository,
    IShipRepository shipRepository,
    IBackgroundJobClient backgroundJobClient) : ITimeManagementService
{
    /// <summary>
    /// Avanza il giorno corrente di 1 unità nel database e accoda un job in background
    /// su Hangfire per aggiornare lo stato delle navi la cui occupazione è terminata.
    /// </summary>
    /// <returns>Il nuovo giorno corrente calcolato.</returns>
    public async Task<int> AdvanceDayAsync()
    {
        // 1. Avanza il giorno virtuale nel database
        int newDay = await systemStateRepository.AdvanceDayAsync();

        // 2. Enqueuea il job di Hangfire per elaborare le navi partite in background in maniera asincrona
        backgroundJobClient.Enqueue<ITimeManagementService>(service => service.ProcessDepartedShipsAsync(newDay));

        return newDay;
    }

    /// <summary>
    /// Elabora le navi che sono già salpate/partite.
    /// Questo metodo viene eseguito in background come worker Hangfire.
    /// </summary>
    /// <param name="currentDay">Il giorno corrente di riferimento per determinare le scadenze delle occupazioni.</param>
    public async Task ProcessDepartedShipsAsync(int currentDay)
    {
        // Questa logica gira in background, gestita da Hangfire
        await shipRepository.UpdateAssignedShipsToDepartedAsync(currentDay);
    }
}
