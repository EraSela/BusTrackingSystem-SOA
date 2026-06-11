using BusTrackingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BusTrackingAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Bus> Buses { get; set; }
        public DbSet<BusLocation> BusLocations { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<BusRoute> Routes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Bus>().HasData(
                new Bus { Id = 1, Name = "Bus 1", PlateNumber = "BUS-0500", DepartureTime = new TimeSpan(5, 0, 0), TotalSeats = 40, IsActive = true },
                new Bus { Id = 2, Name = "Bus 2", PlateNumber = "BUS-0800", DepartureTime = new TimeSpan(8, 0, 0), TotalSeats = 40, IsActive = true },
                new Bus { Id = 3, Name = "Bus 3", PlateNumber = "BUS-1200", DepartureTime = new TimeSpan(12, 0, 0), TotalSeats = 40, IsActive = true },

                new Bus { Id = 4, Name = "Bus 4", PlateNumber = "BUS-1100", DepartureTime = new TimeSpan(11, 0, 0), TotalSeats = 40, IsActive = true },
                new Bus { Id = 5, Name = "Bus 5", PlateNumber = "BUS-1315", DepartureTime = new TimeSpan(13, 15, 0), TotalSeats = 40, IsActive = true },
                new Bus { Id = 6, Name = "Bus 6", PlateNumber = "BUS-1620", DepartureTime = new TimeSpan(16, 20, 0), TotalSeats = 40, IsActive = true },

                new Bus { Id = 7, Name = "Bus 7", PlateNumber = "BUS-T1200", DepartureTime = new TimeSpan(12, 0, 0), TotalSeats = 40, IsActive = true },
                new Bus { Id = 8, Name = "Bus 8", PlateNumber = "BUS-T1415", DepartureTime = new TimeSpan(14, 15, 0), TotalSeats = 40, IsActive = true },
                new Bus { Id = 9, Name = "Bus 9", PlateNumber = "BUS-T1720", DepartureTime = new TimeSpan(17, 20, 0), TotalSeats = 40, IsActive = true },

                new Bus { Id = 10, Name = "Bus 10", PlateNumber = "BUS-1700", DepartureTime = new TimeSpan(17, 0, 0), TotalSeats = 40, IsActive = true },
                new Bus { Id = 11, Name = "Bus 11", PlateNumber = "BUS-2100", DepartureTime = new TimeSpan(21, 0, 0), TotalSeats = 40, IsActive = true }
            );

            modelBuilder.Entity<BusRoute>().HasData(
                new BusRoute { Id = 1, Name = "Struga - Tetovo - Skopje", Origin = "Struga", Destination = "Skopje", ExpectedDurationMinutes = 180, IsActive = true },
                new BusRoute { Id = 2, Name = "Skopje - Tetovo - Struga", Origin = "Skopje", Destination = "Struga", ExpectedDurationMinutes = 180, IsActive = true },
                new BusRoute { Id = 3, Name = "Tetovo - Struga", Origin = "Tetovo", Destination = "Struga", ExpectedDurationMinutes = 120, IsActive = true }
            );

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Reservation>()
                .HasIndex(r => new
                {
                    r.TripId,
                    r.SeatNumber
                })
                .IsUnique()
                .HasFilter("\"TripId\" IS NOT NULL");

            modelBuilder.Entity<BusLocation>()
                .HasOne(bl => bl.Bus)
                .WithMany(b => b.Locations)
                .HasForeignKey(bl => bl.BusId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Bus)
                .WithMany(b => b.Trips)
                .HasForeignKey(t => t.BusId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Driver)
                .WithMany(u => u.DrivenTrips)
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Route)
                .WithMany(r => r.Trips)
                .HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Trip)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BusLocation>()
                .HasOne(l => l.Trip)
                .WithMany(t => t.Locations)
                .HasForeignKey(l => l.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Trip)
                .WithMany()
                .HasForeignKey(n => n.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Reservation)
                .WithMany()
                .HasForeignKey(n => n.ReservationId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new
                {
                    n.TripId,
                    n.UserId,
                    n.ReservationId,
                    n.Type
                })
                .IsUnique();
        }
    }
}
