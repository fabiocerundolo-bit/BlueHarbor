using BlueHarbor.Application.Interfaces;
using BlueHarbor.Infrastructure.Repositories;
using Hangfire;

namespace BlueHarbor.Application.Services;

/// <summary>
/// Service responsible for managing the virtual system time.
/// Handles day advancement and triggers asynchronous jobs to update the status of ships in transit.
/// </summary>
public class TimeManagementService(
    ISystemStateRepository systemStateRepository,
    IShipRepository shipRepository,
    IBackgroundJobClient backgroundJobClient) : ITimeManagementService
{
    /// <summary>
    /// Advances the current day by 1 unit in the database and enqueues a background Hangfire job
    /// to update the status of ships whose occupancy has ended.
    /// </summary>
    /// <returns>The newly calculated current day.</returns>
    public async Task<int> AdvanceDayAsync()
    {
        // 1. Advance the virtual day in the database
        int newDay = await systemStateRepository.AdvanceDayAsync();

        // 2. Enqueue the Hangfire job to process departed ships asynchronously in the background
        backgroundJobClient.Enqueue<ITimeManagementService>(service => service.ProcessDepartedShipsAsync(newDay));

        return newDay;
    }

    /// <summary>
    /// Processes ships that have already departed.
    /// This method is executed in the background as a Hangfire worker.
    /// </summary>
    /// <param name="currentDay">The current reference day used to determine occupancy expiries.</param>
    public async Task ProcessDepartedShipsAsync(int currentDay)
    {
        // This logic runs in the background, managed by Hangfire
        await shipRepository.UpdateAssignedShipsToDepartedAsync(currentDay);
    }
}
