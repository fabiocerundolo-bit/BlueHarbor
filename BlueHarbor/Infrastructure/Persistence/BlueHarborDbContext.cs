namespace BlueHarbor.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using BlueHarbor.Domain.Entities;
using BlueHarbor.Domain.Enums;

public class BlueHarborDbContext : DbContext
{
    public BlueHarborDbContext(DbContextOptions<BlueHarborDbContext> options) : base(options) { }

    public DbSet<Ship> Ships => Set<Ship>();
    public DbSet<Berth> Berths => Set<Berth>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<SystemState> SystemStates => Set<SystemState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Configurazione SystemState (Singleton)
        modelBuilder.Entity<SystemState>()
            .HasData(new SystemState { Id = 1, CurrentDay = 1 });

        // 2. Seed delle 8 Banchine Fisse (Regola di dominio)
        modelBuilder.Entity<Berth>().HasData(
            new Berth { Id = 1, Name = "Berth-XL-1", Size = ShipSize.XL },
            new Berth { Id = 2, Name = "Berth-L-1", Size = ShipSize.L },
            new Berth { Id = 3, Name = "Berth-M-1", Size = ShipSize.M },
            new Berth { Id = 4, Name = "Berth-M-2", Size = ShipSize.M },
            new Berth { Id = 5, Name = "Berth-S-1", Size = ShipSize.S },
            new Berth { Id = 6, Name = "Berth-S-2", Size = ShipSize.S },
            new Berth { Id = 7, Name = "Berth-S-3", Size = ShipSize.S },
            new Berth { Id = 8, Name = "Berth-S-4", Size = ShipSize.S }
        );

        // 3. Configurazione Relazioni e Vincoli
        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Ship)
            .WithMany()
            .HasForeignKey(a => a.ShipId)
            .OnDelete(DeleteBehavior.Restrict); // Evita cancellazioni a cascata accidentali

        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Berth)
            .WithMany(b => b.Assignments)
            .HasForeignKey(a => a.BerthId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<Ship>()
            .HasOne(s => s.AssignedBerth)
            .WithMany()
            .HasForeignKey(s => s.AssignedBerthId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
