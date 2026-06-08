using BlueHarbor.Application.Interfaces;
using BlueHarbor.Domain.Enums;
using BlueHarbor.Infrastructure.Repositories;

namespace BlueHarbor.Application.Services;

public class BackgroundJobService(IShipRepository shipRepository) : IBackgroundJobService
{
    public async Task ProcessDepartedShipsAsync(int currentDay)
    {
        var assignedShips = await shipRepository.GetByStatusAsync("Assigned");
        
        // Dobbiamo caricare le occupazioni per calcolare se sono partite
        // La logica è stata spostata nel repository per efficienza
        await shipRepository.UpdateAssignedShipsToDepartedAsync(currentDay);
    }
}
