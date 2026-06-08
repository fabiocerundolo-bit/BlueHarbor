namespace BlueHarbor.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using BlueHarbor.Domain.Entities;

public class BlueHarborDbContext : DbContext
{
    public BlueHarborDbContext(DbContextOptions<BlueHarborDbContext> options) : base(options) { }

    public DbSet<Ruolo> Ruoli => Set<Ruolo>();
    public DbSet<Dimensione> Dimensioni => Set<Dimensione>();
    public DbSet<Utente> Utenti => Set<Utente>();
    public DbSet<Banchina> Banchine => Set<Banchina>();
    public DbSet<Nave> Navi => Set<Nave>();
    public DbSet<Occupazione> Occupazioni => Set<Occupazione>();
    public DbSet<SystemState> SystemStates => Set<SystemState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mappatura Nomi Tabelle
        modelBuilder.Entity<Ruolo>().ToTable("Ruolo").HasKey(r => r.IdRuolo);
        modelBuilder.Entity<Dimensione>().ToTable("Dimensione").HasKey(d => d.IdDimensione);
        modelBuilder.Entity<Utente>().ToTable("Utente").HasKey(u => u.IdUtente);
        modelBuilder.Entity<Banchina>().ToTable("Banchina").HasKey(b => b.IdBanchina);
        modelBuilder.Entity<Nave>().ToTable("Nave").HasKey(n => n.IdNave);
        modelBuilder.Entity<Occupazione>().ToTable("Occupazione").HasKey(o => o.IdOccupazione);

        // 1. Configurazione SystemState (Singleton)
        modelBuilder.Entity<SystemState>()
            .HasData(new SystemState { Id = 1, CurrentDay = 1 });

        // 2. Seed dei dati da SQLQuery2.sql
        modelBuilder.Entity<Ruolo>().HasData(
            new Ruolo { IdRuolo = 1, NomeRuolo = "Operatore" },
            new Ruolo { IdRuolo = 2, NomeRuolo = "Scheduler" }
        );

        modelBuilder.Entity<Dimensione>().HasData(
            new Dimensione { IdDimensione = 1, NomeDimensione = "XL" },
            new Dimensione { IdDimensione = 2, NomeDimensione = "L" },
            new Dimensione { IdDimensione = 3, NomeDimensione = "M" },
            new Dimensione { IdDimensione = 4, NomeDimensione = "S" }
        );

        modelBuilder.Entity<Banchina>().HasData(
            new Banchina { IdBanchina = 1, NomeBanchina = "Banchina XL1", IdDimensione = 1 },
            new Banchina { IdBanchina = 2, NomeBanchina = "Banchina L1", IdDimensione = 2 },
            new Banchina { IdBanchina = 3, NomeBanchina = "Banchina M1", IdDimensione = 3 },
            new Banchina { IdBanchina = 4, NomeBanchina = "Banchina M2", IdDimensione = 3 },
            new Banchina { IdBanchina = 5, NomeBanchina = "Banchina S1", IdDimensione = 4 },
            new Banchina { IdBanchina = 6, NomeBanchina = "Banchina S2", IdDimensione = 4 },
            new Banchina { IdBanchina = 7, NomeBanchina = "Banchina S3", IdDimensione = 4 },
            new Banchina { IdBanchina = 8, NomeBanchina = "Banchina S4", IdDimensione = 4 }
        );

        // Seed un utente di default per i ruoli (opzionale, per permettere alle navi di avere un IdUtente)
        modelBuilder.Entity<Utente>().HasData(
            new Utente { IdUtente = 1, Nome = "Admin", Email = "admin@blueharbor.com", Password = "admin", IdRuolo = 1 }
        );

        // Relazioni
        modelBuilder.Entity<Utente>()
            .HasOne(u => u.Ruolo)
            .WithMany()
            .HasForeignKey(u => u.IdRuolo);

        modelBuilder.Entity<Banchina>()
            .HasOne(b => b.Dimensione)
            .WithMany()
            .HasForeignKey(b => b.IdDimensione);

        modelBuilder.Entity<Nave>()
            .HasOne(n => n.Dimensione)
            .WithMany()
            .HasForeignKey(n => n.IdDimensione);

        modelBuilder.Entity<Nave>()
            .HasOne(n => n.Utente)
            .WithMany()
            .HasForeignKey(n => n.IdUtente);

        modelBuilder.Entity<Occupazione>()
            .HasOne(o => o.Nave)
            .WithMany()
            .HasForeignKey(o => o.IdNave)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Occupazione>()
            .HasOne(o => o.Banchina)
            .WithMany(b => b.Occupazioni)
            .HasForeignKey(o => o.IdBanchina)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Occupazione>()
            .HasOne(o => o.Utente)
            .WithMany()
            .HasForeignKey(o => o.IdUtente);
    }
}
