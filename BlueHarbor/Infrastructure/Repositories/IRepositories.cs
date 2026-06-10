using BlueHarbor.Application.DTOs;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Domain.Enums;

namespace BlueHarbor.Infrastructure.Repositories;

/// <summary>
/// Interfaccia del repository per l'entità Nave.
/// Gestisce la persistenza e le query relative alle navi e ai loro attracchi.
/// </summary>
public interface IShipRepository
{
    /// <summary>
    /// Recupera una nave tramite il suo ID univoco.
    /// </summary>
    Task<Nave?> GetByIdAsync(int id);

    /// <summary>
    /// Recupera tutte le navi che si trovano in uno specifico stato.
    /// </summary>
    Task<IEnumerable<Nave>> GetByStatusAsync(string status);

    /// <summary>
    /// Recupera le navi pendenti proiettandole in un DTO leggero per lo scheduler.
    /// </summary>
    Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync();

    /// <summary>
    /// Recupera la lista di tutte le navi registrate proiettate in ShipDto con dettagli di occupazione.
    /// </summary>
    Task<IEnumerable<ShipDto>> GetAllShipsAsync();

    /// <summary>
    /// Aggiunge una nuova nave al database.
    /// </summary>
    Task AddAsync(Nave ship);

    /// <summary>
    /// Aggiorna i dati di una nave esistente.
    /// </summary>
    Task UpdateAsync(Nave ship);

    /// <summary>
    /// Aggiorna un intervallo di navi.
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<Nave> ships);

    /// <summary>
    /// Registra l'assegnazione temporale (occupazione) di una nave ad una banchina.
    /// </summary>
    Task AddAssignmentAsync(Occupazione assignment);

    /// <summary>
    /// Trova le navi assegnate la cui sosta è terminata al giorno specificato e ne aggiorna lo stato in "Departed".
    /// </summary>
    Task<int> UpdateAssignedShipsToDepartedAsync(int currentDay);
}

/// <summary>
/// Interfaccia del repository per l'entità Banchina.
/// </summary>
public interface IBerthRepository
{
    /// <summary>
    /// Recupera una banchina tramite ID comprensiva delle sue occupazioni e dettagli navi.
    /// </summary>
    Task<Banchina?> GetByIdAsync(int id);

    /// <summary>
    /// Recupera tutte le banchine incluse le relazioni per taglia e occupazioni.
    /// </summary>
    Task<IEnumerable<Banchina>> GetAllWithAssignmentsAsync();

    /// <summary>
    /// Recupera tutte le banchine formattate come BerthDto per l'interfaccia utente.
    /// </summary>
    Task<IEnumerable<BerthDto>> GetBerthsWithAssignmentsAsync();
}

/// <summary>
/// Interfaccia del repository per gestire lo stato globale e il tempo virtuale del porto.
/// </summary>
public interface ISystemStateRepository
{
    /// <summary>
    /// Recupera l'unico record dello stato di sistema.
    /// </summary>
    Task<SystemState> GetAsync();

    /// <summary>
    /// Aggiorna il record dello stato di sistema.
    /// </summary>
    Task UpdateAsync(SystemState state);

    /// <summary>
    /// Incrementa il giorno virtuale di sistema di 1.
    /// </summary>
    Task<int> AdvanceDayAsync();
}
