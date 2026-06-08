using BlueHarbor.Application.Interfaces;
using BlueHarbor.Domain.Enums;
using BlueHarbor.Infrastructure.Repositories;

namespace BlueHarbor.Application.Services;

public class BackgroundJobService(IShipRepository shipRepository) : IBackgroundJobService
{
    public async Task ProcessDepartedShipsAsync(int currentDay)
    {
        var assignedShips = await shipRepository.GetByStatusAsync(ShipStatus.Assigned);
        
        var departedShips = assignedShips.Where(s => s.StartDay.HasValue && 
                                                     (s.StartDay.Value + s.DurationDays - 1) < currentDay).ToList();

        if (departedShips.Any())
        {
            foreach (var ship in departedShips)
            {
                ship.Status = ShipStatus.Departed;
            }

            await shipRepository.UpdateRangeAsync(departedShips);
        }
    }
}
