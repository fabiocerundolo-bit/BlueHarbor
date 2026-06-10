using BlueHarbor.Application.DTOs;

namespace BlueHarbor.Application.Interfaces;

public interface IShipService
{
    Task<ShipResponseDto> CreateShipAsync(CreateShipRequest request);
}

public interface ITimeManagementService
{
    Task<int> AdvanceDayAsync();
    Task ProcessDepartedShipsAsync(int currentDay);
}

public interface ISchedulerService
{
    Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync();
    Task<IEnumerable<BerthDto>> GetBerthsAsync();
    Task<AssignmentResponseDto> AssignShipToBerthAsync(int shipId, int berthId);
}