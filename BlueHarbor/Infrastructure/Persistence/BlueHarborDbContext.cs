namespace BlueHarbor.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using BlueHarbor.Domain.Entities;

public class BlueHarborDbContext : DbContext
{
    public BlueHarborDbContext(DbContextOptions<BlueHarborDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Size> Sizes => Set<Size>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Berth> Berths => Set<Berth>();
    public DbSet<Ship> Ships => Set<Ship>();
    public DbSet<Occupancy> Occupancies => Set<Occupancy>();
    public DbSet<SystemState> SystemStates => Set<SystemState>();

    public DbSet<ShipList> ShipLists => Set<ShipList>(); // Add DbSet for ShipList entity

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table Name Mappings
        modelBuilder.Entity<Role>().ToTable("Role").HasKey(r => r.RoleId);
        modelBuilder.Entity<Size>().ToTable("Size").HasKey(d => d.SizeId);
        modelBuilder.Entity<User>().ToTable("User").HasKey(u => u.UserId);
        modelBuilder.Entity<Berth>().ToTable("Berth").HasKey(b => b.BerthId);
        modelBuilder.Entity<Ship>().ToTable("Ship").HasKey(n => n.ShipId);
        modelBuilder.Entity<Occupancy>().ToTable("Occupancy").HasKey(o => o.OccupancyId);

        // 1. SystemState configuration (Singleton)
        modelBuilder.Entity<SystemState>()
            .HasData(new SystemState { Id = 1, CurrentDay = 1 });

        // 2. Seed data
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Operator" },
            new Role { RoleId = 2, RoleName = "Scheduler" }
        );

        modelBuilder.Entity<Size>().HasData(
            new Size { SizeId = 1, SizeName = "XL" },
            new Size { SizeId = 2, SizeName = "L" },
            new Size { SizeId = 3, SizeName = "M" },
            new Size { SizeId = 4, SizeName = "S" }
        );

        modelBuilder.Entity<Berth>().HasData(
            new Berth { BerthId = 1, BerthName = "Berth XL1", SizeId = 1 },
            new Berth { BerthId = 2, BerthName = "Berth L1",  SizeId = 2 },
            new Berth { BerthId = 3, BerthName = "Berth M1",  SizeId = 3 },
            new Berth { BerthId = 4, BerthName = "Berth M2",  SizeId = 3 },
            new Berth { BerthId = 5, BerthName = "Berth S1",  SizeId = 4 },
            new Berth { BerthId = 6, BerthName = "Berth S2",  SizeId = 4 },
            new Berth { BerthId = 7, BerthName = "Berth S3",  SizeId = 4 },
            new Berth { BerthId = 8, BerthName = "Berth S4",  SizeId = 4 }
        );

        // Seed a default admin user (required for ships to have a UserId)
        modelBuilder.Entity<User>().HasData(
            new User { UserId = 1, Name = "Admin", Email = "admin@blueharbor.com", Password = "admin", RoleId = 1 }
        );

        // Relationships
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId);

        modelBuilder.Entity<Berth>()
            .HasOne(b => b.Size)
            .WithMany()
            .HasForeignKey(b => b.SizeId);

        modelBuilder.Entity<Ship>()
            .HasOne(n => n.Size)
            .WithMany()
            .HasForeignKey(n => n.SizeId);

        modelBuilder.Entity<Ship>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId);

        modelBuilder.Entity<Occupancy>()
            .HasOne(o => o.Ship)
            .WithMany()
            .HasForeignKey(o => o.ShipId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Occupancy>()
            .HasOne(o => o.Berth)
            .WithMany(b => b.Occupancies)
            .HasForeignKey(o => o.BerthId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Occupancy>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId);

        modelBuilder.Entity<ShipList>(entity => // Configure ShipList entity
        {
            entity.ToTable("ShipList");
            entity.HasKey(e => e.IdShipList);
            entity.HasOne(e => e.Size)
                    .WithMany(d => d.ShipLists)
                    .HasForeignKey(e => e.SizeId)
                    .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
