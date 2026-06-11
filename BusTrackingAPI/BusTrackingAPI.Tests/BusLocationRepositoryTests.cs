using BusTrackingAPI.Data;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

namespace BusTrackingAPI.Tests;

public class BusLocationRepositoryTests
{
    [Fact]
    public async Task GetLatestByBusIdAsync_ShouldReturnMostRecentLocation()
    {
        await using var context = CreateContext();
        var bus = CreateBus();
        var older = CreateLocation(bus.Id, DateTime.UtcNow.AddMinutes(-5));
        var latest = CreateLocation(bus.Id, DateTime.UtcNow);
        context.Buses.Add(bus);
        context.BusLocations.AddRange(older, latest);
        await context.SaveChangesAsync();
        var repository = new BusLocationRepository(context);

        var result = await repository.GetLatestByBusIdAsync(bus.Id);

        Assert.NotNull(result);
        Assert.Equal(latest.Id, result.Id);
        Assert.Equal(latest.Timestamp, result.Timestamp);
    }

    [Fact]
    public async Task CreateAsync_ShouldSaveBusLocation()
    {
        await using var context = CreateContext();
        var bus = CreateBus();
        context.Buses.Add(bus);
        await context.SaveChangesAsync();
        var repository = new BusLocationRepository(context);
        var location = CreateLocation(bus.Id, DateTime.UtcNow);

        var result = await repository.CreateAsync(location);

        Assert.True(result.Id > 0);
        Assert.Equal(bus.Id, result.BusId);
        Assert.NotNull(result.Bus);
        Assert.Equal(1, await context.BusLocations.CountAsync());
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static Bus CreateBus()
    {
        return new Bus
        {
            Id = 30,
            Name = "Location Test Bus",
            PlateNumber = "LOC-01",
            TotalSeats = 40,
            IsActive = true
        };
    }

    private static BusLocation CreateLocation(
        int busId,
        DateTime timestamp)
    {
        return new BusLocation
        {
            BusId = busId,
            Latitude = 41.18,
            Longitude = 20.68,
            Speed = 40,
            Timestamp = timestamp
        };
    }
}
