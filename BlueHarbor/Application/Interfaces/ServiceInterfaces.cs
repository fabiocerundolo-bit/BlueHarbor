using BlueHarbor.Application.DTOs;

namespace BlueHarbor.Application.Interfaces;

/// <summary>
/// Interfaccia per il servizio di gestione delle navi (registrazione).
/// </summary>
public interface IShipService
{
    /// <summary>
    /// Crea e registra una nuova nave nel sistema con dati generati in maniera casuale.
    /// </summary>
    Task<ShipResponseDto> CreateShipAsync(CreateShipRequest request);
}

/// <summary>
/// Interfaccia per il servizio di gestione temporale e avanzamento giorno virtuale.
/// </summary>
public interface ITimeManagementService
{
    /// <summary>
    /// Avanza il giorno virtuale di sistema di 1.
    /// </summary>
    Task<int> AdvanceDayAsync();

    /// <summary>
    /// Processa le navi la cui occupazione scade nel giorno specificato per impostarle come "Departed".
    /// </summary>
    Task ProcessDepartedShipsAsync(int currentDay);
}

/// <summary>
/// Interfaccia per il servizio di pianificazione degli attracchi.
/// </summary>
public interface ISchedulerService
{
    /// <summary>
    /// Restituisce la lista di navi in attesa di assegnazione.
    /// </summary>
    Task<IEnumerable<PendingShipDto>> GetPendingShipsAsync();

    /// <summary>
    /// Restituisce la lista di banchine con le rispettive occupazioni programmate.
    /// </summary>
    Task<IEnumerable<BerthDto>> GetBerthsAsync();

    /// <summary>
    /// Assegna una nave pendente ad una determinata banchina.
    /// </summary>
    Task<AssignmentResponseDto> AssignShipToBerthAsync(int shipId, int berthId);
}