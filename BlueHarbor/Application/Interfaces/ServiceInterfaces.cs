using BlueHarbor.Application.DTOs;

namespace BlueHarbor.Application.Interfaces;

public interface IShipService
{
    Task<ShipResponseDto> CreateShipAsync(CreateShipRequest request);
    Task<AssignmentDto> AssignBerthAsync(int shipId, int berthId);
}

public interface ISystemService
{
    Task<int> GetCurrentDayAsync();
    Task<int> AdvanceDayAsync(); // Gestisce il "Next Day"
}

public interface ITimeManagementService
{
    Task<int> AdvanceDayAsync();
    Task ProcessDepartedShipsAsync(int currentDay);
}

public interface ISchedulerService
{
    Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync();
    Task<AssignmentResponseDto> AssignShipToBerthAsync(int shipId, int berthId);
}

public interface IBackgroundJobService
{
    // Job enqueueato dopo l'avanzamento del giorno
    Task ProcessDepartedShipsAsync(int currentDay); 
}
