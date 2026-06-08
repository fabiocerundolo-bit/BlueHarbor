using BlueHarbor.Application.DTOs;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Domain.Enums;

namespace BlueHarbor.Infrastructure.Repositories;

public interface IShipRepository
{
    Task<Ship?> GetByIdAsync(int id);
    Task<IEnumerable<Ship>> GetByStatusAsync(ShipStatus status);
    Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync();
    Task AddAsync(Ship ship);
    Task UpdateAsync(Ship ship);
    Task UpdateRangeAsync(IEnumerable<Ship> ships);
    Task AddAssignmentAsync(Assignment assignment);
    Task<int> UpdateAssignedShipsToDepartedAsync(int currentDay);
}

public interface IBerthRepository
{
    Task<Berth?> GetByIdAsync(int id);
    Task<IEnumerable<Berth>> GetAllWithAssignmentsAsync();
}

public interface ISystemStateRepository
{
    Task<SystemState> GetAsync();
    Task UpdateAsync(SystemState state);
    Task<int> AdvanceDayAsync();
}
