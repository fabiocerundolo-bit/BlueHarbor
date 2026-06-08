using BlueHarbor.Application.Interfaces;
using BlueHarbor.Infrastructure.Repositories;
using Hangfire;

namespace BlueHarbor.Application.Services;

public class TimeManagementService(
    ISystemStateRepository systemStateRepository,
    IShipRepository shipRepository,
    IBackgroundJobClient backgroundJobClient) : ITimeManagementService
{
    public async Task<int> AdvanceDayAsync()
    {
        // 1. Avanza il giorno virtuale nel database
        int newDay = await systemStateRepository.AdvanceDayAsync();

        // 2. Enqueuea il job di Hangfire per elaborare le navi partite in background
        backgroundJobClient.Enqueue<ITimeManagementService>(service => service.ProcessDepartedShipsAsync(newDay));

        return newDay;
    }

    public async Task ProcessDepartedShipsAsync(int currentDay)
    {
        // Questa logica gira in background, gestita da Hangfire
        await shipRepository.UpdateAssignedShipsToDepartedAsync(currentDay);
    }
}
