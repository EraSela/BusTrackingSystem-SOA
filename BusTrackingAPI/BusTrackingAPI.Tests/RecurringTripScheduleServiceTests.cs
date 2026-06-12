using BusTrackingAPI.Data;
using BusTrackingAPI.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace BusTrackingAPI.Tests;

public class RecurringTripScheduleServiceTests
{
    [Fact]
    public async Task EnsureUpcomingTripsAsync_ShouldCreateDailyAndMondayServices()
    {
        await using var context = CreateContext();
        var service = new RecurringTripScheduleService(context);
        var mondayMidnightInSkopje = new DateTime(
            2026,
            6,
            14,
            22,
            0,
            0,
            DateTimeKind.Utc);

        var created = await service.EnsureUpcomingTripsAsync(
            mondayMidnightInSkopje,
            daysAhead: 0);
        var trips = await context.Trips
            .Include(trip => trip.Route)
            .OrderBy(trip => trip.ScheduledDeparture)
            .ToListAsync();

        Assert.Equal(11, created);
        Assert.Equal(11, trips.Count);
        Assert.Equal(
            new[]
            {
                "Skopje:11:00",
                "Skopje:13:15",
                "Skopje:16:20",
                "Skopje:21:00",
                "Struga:05:00",
                "Struga:08:00",
                "Struga:12:00",
                "Struga:17:00",
                "Tetovo:12:00",
                "Tetovo:14:15",
                "Tetovo:17:20"
            },
            trips
                .Select(trip =>
                    $"{trip.Route!.Origin}:{ToSkopjeTime(trip.ScheduledDeparture):HH:mm}")
                .OrderBy(value => value)
                .ToArray());
    }

    [Fact]
    public async Task EnsureUpcomingTripsAsync_ShouldExcludeLimitedServicesOnTuesday()
    {
        await using var context = CreateContext();
        var service = new RecurringTripScheduleService(context);
        var tuesdayMidnightInSkopje = new DateTime(
            2026,
            6,
            15,
            22,
            0,
            0,
            DateTimeKind.Utc);

        var created = await service.EnsureUpcomingTripsAsync(
            tuesdayMidnightInSkopje,
            daysAhead: 0);
        var localDepartureHours = await context.Trips
            .Select(trip => trip.ScheduledDeparture)
            .ToListAsync();

        Assert.Equal(9, created);
        Assert.DoesNotContain(
            localDepartureHours,
            departure => ToSkopjeTime(departure).Hour == 21);
        Assert.DoesNotContain(
            context.Trips.Include(trip => trip.Route),
            trip =>
                trip.Route!.Origin == "Struga" &&
                ToSkopjeTime(trip.ScheduledDeparture).Hour == 17);
    }

    [Fact]
    public async Task EnsureUpcomingTripsAsync_ShouldBeIdempotentAndRestoreRequiredBus()
    {
        await using var context = CreateContext();
        var service = new RecurringTripScheduleService(context);
        var mondayMidnightInSkopje = new DateTime(
            2026,
            6,
            14,
            22,
            0,
            0,
            DateTimeKind.Utc);

        var firstCreated = await service.EnsureUpcomingTripsAsync(
            mondayMidnightInSkopje,
            daysAhead: 0);
        var secondCreated = await service.EnsureUpcomingTripsAsync(
            mondayMidnightInSkopje,
            daysAhead: 0);

        Assert.Equal(11, firstCreated);
        Assert.Equal(0, secondCreated);
        Assert.Equal(11, await context.Trips.CountAsync());
        Assert.True(await context.Buses.AnyAsync(
            bus => bus.PlateNumber == "BUS-2100"));
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static DateTime ToSkopjeTime(DateTime utc)
    {
        TimeZoneInfo timeZone;
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");
        }
        catch (TimeZoneNotFoundException)
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(
                "Central European Standard Time");
        }

        return TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utc, DateTimeKind.Utc),
            timeZone);
    }
}
