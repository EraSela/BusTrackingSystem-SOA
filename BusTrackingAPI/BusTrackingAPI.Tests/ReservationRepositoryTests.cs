using BusTrackingAPI.Data;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace BusTrackingAPI.Tests;

public class ReservationRepositoryTests
{
    [Fact]
    public async Task IsSeatTakenAsync_ShouldReturnTrue_WhenSeatAlreadyExists()
    {
        await using var context = CreateContext();
        var data = CreateData();
        context.AddRange(data.Bus, data.User, data.Trip, data.Reservation);
        await context.SaveChangesAsync();

        var result = await new ReservationRepository(context)
            .IsSeatTakenAsync(data.Trip.Id, data.Reservation.SeatNumber);

        Assert.True(result);
    }

    [Fact]
    public async Task IsSeatTakenAsync_ShouldReturnFalse_WhenSeatIsAvailable()
    {
        await using var context = CreateContext();
        var result = await new ReservationRepository(context)
            .IsSeatTakenAsync(99, 8);

        Assert.False(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldSaveReservation()
    {
        await using var context = CreateContext();
        var data = CreateData();
        context.AddRange(data.Bus, data.User, data.Trip);
        await context.SaveChangesAsync();

        var result = await new ReservationRepository(context)
            .CreateAsync(data.Reservation);

        Assert.True(result.Id > 0);
        Assert.Equal(data.Trip.Id, result.TripId);
        Assert.NotNull(result.Trip);
        Assert.NotNull(result.User);
        Assert.Equal(1, await context.Reservations.CountAsync());
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static (Bus Bus, User User, Trip Trip, Reservation Reservation) CreateData()
    {
        var bus = new Bus { Id = 20, Name = "Repository Bus", PlateNumber = "REPO-01", TotalSeats = 40, IsActive = true };
        var user = new User { Id = 20, FullName = "Repository User", Email = "repository@test.local", PasswordHash = "hash" };
        var trip = new Trip
        {
            Id = 20,
            BusId = bus.Id,
            Bus = bus,
            DeviceId = "DEVICE-01",
            ScheduledDeparture = DateTime.UtcNow.AddDays(1),
            ScheduledArrival = DateTime.UtcNow.AddDays(1).AddHours(3)
        };
        var reservation = new Reservation
        {
            TripId = trip.Id,
            Trip = trip,
            UserId = user.Id,
            User = user,
            SeatNumber = 4,
            PickupPlaceName = "Struga Bus Station",
            PickupLatitude = 41.17799,
            PickupLongitude = 20.67784,
            QrCode = Guid.NewGuid().ToString()
        };
        return (bus, user, trip, reservation);
    }
}
