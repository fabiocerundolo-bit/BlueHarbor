using BlueHarbor.Application.Interfaces;
using BlueHarbor.Infrastructure.Repositories;
using Hangfire;

namespace BlueHarbor.Application.Services;

public class SystemService(
    ISystemStateRepository stateRepository,
    IBackgroundJobClient backgroundJobClient) : ISystemService
{
    public async Task<int> GetCurrentDayAsync()
    {
        var state = await stateRepository.GetAsync();
        return state.CurrentDay;
    }

    public async Task<int> AdvanceDayAsync()
    {
        var state = await stateRepository.GetAsync();
        state.CurrentDay++;
        await stateRepository.UpdateAsync(state);

        // Enqueuea il job per aggiornare gli stati delle navi in background
        backgroundJobClient.Enqueue<IBackgroundJobService>(service => service.ProcessDepartedShipsAsync(state.CurrentDay));

        return state.CurrentDay;
    }
}
