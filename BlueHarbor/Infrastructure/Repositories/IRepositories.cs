using BlueHarbor.Application.DTOs;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Domain.Enums;

namespace BlueHarbor.Infrastructure.Repositories;

public interface IShipRepository
{
    Task<Nave?> GetByIdAsync(int id);
    Task<IEnumerable<Nave>> GetByStatusAsync(string status);
    Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync();
    Task<IEnumerable<ShipDto>> GetAllShipsAsync();
    Task AddAsync(Nave ship);
    Task UpdateAsync(Nave ship);
    Task UpdateRangeAsync(IEnumerable<Nave> ships);
    Task AddAssignmentAsync(Occupazione assignment);
    Task<int> UpdateAssignedShipsToDepartedAsync(int currentDay);
}

public interface IBerthRepository
{
    Task<Banchina?> GetByIdAsync(int id);
    Task<IEnumerable<Banchina>> GetAllWithAssignmentsAsync();
    Task<IEnumerable<BerthDto>> GetBerthsWithAssignmentsAsync();
}

public interface ISystemStateRepository
{
    Task<SystemState> GetAsync();
    Task UpdateAsync(SystemState state);
    Task<int> AdvanceDayAsync();
}
