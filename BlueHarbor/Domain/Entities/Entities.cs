namespace BlueHarbor.Domain.Entities;

using BlueHarbor.Domain.Enums;

/// <summary>
/// Represents the security role associated with a user (e.g. Operator, Scheduler).
/// </summary>
public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}

/// <summary>
/// Represents the size of a ship or a berth (e.g. XL, L, M, S).
/// </summary>
public class Size
{
    public int SizeId { get; set; }
    public string SizeName { get; set; } = string.Empty;

    public ICollection<Berth> Berths { get; set; } = new List<Berth>();
    public ICollection<ListaNavi> ListaNavi { get; set; } = new List<ListaNavi>();
}

/// <summary>
/// Represents a registered user who can perform operations in the harbor system.
/// </summary>
public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public ICollection<Ship> Ships { get; set; } = new List<Ship>();
}

/// <summary>
/// Represents a physical berth in the harbor prepared for ships of a specific size to dock.
/// </summary>
public class Berth
{
    public int BerthId { get; set; }
    public string BerthName { get; set; } = string.Empty;
    public int SizeId { get; set; }
    public Size Size { get; set; } = null!;
    
    // List of all temporal occupancies associated with this berth
    public ICollection<Occupancy> Occupancies { get; set; } = new List<Occupancy>();
}

/// <summary>
/// Represents the lookup list of ship names grouped by size.
/// </summary>
public class ListaNavi
{
    public int IdListaNavi { get; set; }
    public string NomeNave { get; set; } = string.Empty;
    public int FK_Id_Dimensione { get; set; }

    public Size Dimensione { get; set; } = null!;
    public ICollection<Ship> Navi { get; set; } = new List<Ship>();
}

/// <summary>
/// Represents a ship registered in the application with its docking preferences and current status.
/// </summary>
public class Ship
{
    public int ShipId { get; set; }
    public int ArrivalDay { get; set; }
    public int DurationDays { get; set; }
    
    // Current status of the ship: 'Pending' (waiting), 'Assigned' (assigned), 'Departed' (departed)
    public string Status { get; set; } = "Pending"; 
    public string? Notes { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int IdListaNavi { get; set; }
    public ListaNavi ListaNavi { get; set; } = null!;
    public ICollection<Occupancy> Occupancies { get; set; } = new List<Occupancy>();
    public string? CustomName { get; set; }
}

/// <summary>
/// Represents the temporal occupancy of a berth by a ship starting from a specific start day.
/// </summary>
public class Occupancy
{
    public int OccupancyId { get; set; }
    public int StartDay { get; set; }
    public int ShipId { get; set; }
    public Ship Ship { get; set; } = null!;
    public int BerthId { get; set; }
    public Berth Berth { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}

/// <summary>
/// Represents the global system state, including the current virtual day.
/// Managed as a singleton entity (single row with ID=1).
/// </summary>
public class SystemState
{
    public int Id { get; set; } = 1;
    public int CurrentDay { get; set; } = 1;
}



