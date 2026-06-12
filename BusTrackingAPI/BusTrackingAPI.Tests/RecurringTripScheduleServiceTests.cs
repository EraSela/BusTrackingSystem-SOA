using BusTrackingAPI.Data;
using BusTrackingAPI.Models;
using BusTrackingAPI.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace BusTrackingAPI.Tests;

public class RecurringTripScheduleServiceTests
{
    [Fact]
    public void GetTimetable_ShouldReturnExactlyElevenOptions()
    {
        using var context = CreateContext();
        var service = new RecurringTripScheduleService(context);

        var timetable = service.GetTimetable();

        Assert.Equal(11, timetable.Count);
        Assert.Equal(
            new[] { "05:00", "08:00", "12:00", "17:00" },
            timetable
                .Where(item => item.Origin == "Struga")
                .Select(item => item.DepartureTime)
                .ToArray());
        Assert.Equal(
            new[] { 0, 1, 5 },
            timetable.Single(item => item.Id == "struga-skopje-1700")
                .AvailableDays.OrderBy(day => day).ToArray());
    }

    [Fact]
    public async Task GetOrCreateTripAsync_ShouldCreateOnlySelectedDate()
    {
        await using var context = CreateContext();
        var service = new RecurringTripScheduleService(context);
        var now = new DateTime(2026, 6, 12, 8, 0, 0, DateTimeKind.Utc);

        var first = await service.GetOrCreateTripAsync(
            "struga-skopje-1200",
            new DateOnly(2026, 6, 13),
            now);
        var second = await service.GetOrCreateTripAsync(
            "struga-skopje-1200",
            new DateOnly(2026, 6, 13),
            now);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await context.Trips.CountAsync());
    }

    [Fact]
    public async Task GetOrCreateTripAsync_ShouldRejectLimitedTripOnTuesday()
    {
        await using var context = CreateContext();
        var service = new RecurringTripScheduleService(context);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetOrCreateTripAsync(
                "skopje-struga-2100",
                new DateOnly(2026, 6, 16),
                new DateTime(2026, 6, 12, 8, 0, 0, DateTimeKind.Utc)));

        Assert.Contains("Monday, Friday, and Sunday", exception.Message);
        Assert.Empty(context.Trips);
    }

    [Fact]
    public async Task RemoveUnusedGeneratedTripsAsync_ShouldKeepReservedTrip()
    {
        await using var context = CreateContext();
        var bus = new Bus
        {
            Name = "Bus",
            PlateNumber = "BUS-0500",
            TotalSeats = 40,
            IsActive = true
        };
        var route = new BusRoute
        {
            Name = "Struga - Tetovo - Skopje",
            Origin = "Struga",
            Destination = "Skopje",
            ExpectedDurationMinutes = 180,
            IsActive = true
        };
        var unused = CreateTrip(bus, route, "SIM808_01");
        var reserved = CreateTrip(bus, route, "SIM808_02");
        reserved.Reservations.Add(new Reservation
        {
            UserId = 1,
            SeatNumber = 1,
            PickupPlaceName = "Struga",
            PickupLatitude = 41.17,
            PickupLongitude = 20.68
        });
        context.Users.Add(new User
        {
            Id = 1,
            FullName = "Passenger",
            Email = "passenger@test.local",
            PasswordHash = "hash",
            Role = UserRole.Passenger
        });
        context.Trips.AddRange(unused, reserved);
        await context.SaveChangesAsync();
        var service = new RecurringTripScheduleService(context);

        var removed = await service.RemoveUnusedGeneratedTripsAsync(
            DateTime.UtcNow);

        Assert.Equal(1, removed);
        Assert.Equal(reserved.Id, Assert.Single(context.Trips).Id);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Trip CreateTrip(
        Bus bus,
        BusRoute route,
        string deviceId)
    {
        return new Trip
        {
            Bus = bus,
            Route = route,
            DeviceId = deviceId,
            ScheduledDeparture = DateTime.UtcNow.AddDays(5),
            ScheduledArrival = DateTime.UtcNow.AddDays(5).AddHours(3),
            Status = TripStatus.Scheduled
        };
    }
}
