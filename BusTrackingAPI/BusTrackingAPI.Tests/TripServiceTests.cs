using BusTrackingAPI.DTOs;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using BusTrackingAPI.Services.Implementations;
using Moq;

namespace BusTrackingAPI.Tests;

public class TripServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCalculateArrivalAndAssignResources()
    {
        var tripRepo = new Mock<ITripRepository>();
        var busRepo = new Mock<IBusRepository>();
        var userRepo = new Mock<IUserRepository>();
        var routeRepo = new Mock<IRouteRepository>();
        var departure = DateTime.UtcNow.AddDays(1);
        var bus = CreateBus(1, "Test Bus");
        var driver = new User
        {
            Id = 7,
            FullName = "Test Driver",
            Email = "driver@test.local",
            PasswordHash = "hash",
            Role = UserRole.Driver,
            IsActive = true
        };
        var route = new BusRoute
        {
            Id = 1,
            Name = "Struga - Skopje",
            Origin = "Struga",
            Destination = "Skopje",
            ExpectedDurationMinutes = 180,
            IsActive = true
        };
        var request = new CreateTripDTO
        {
            BusId = bus.Id,
            DriverId = driver.Id,
            RouteId = route.Id,
            DeviceId = "TRACKER-01",
            ScheduledDeparture = departure
        };

        busRepo.Setup(repo => repo.GetByIdAsync(bus.Id)).ReturnsAsync(bus);
        userRepo.Setup(repo => repo.GetByIdAsync(driver.Id)).ReturnsAsync(driver);
        routeRepo.Setup(repo => repo.GetByIdAsync(route.Id)).ReturnsAsync(route);
        tripRepo.Setup(repo => repo.CreateAsync(It.IsAny<Trip>()))
            .ReturnsAsync((Trip trip) =>
            {
                trip.Id = 10;
                trip.Bus = bus;
                trip.Driver = driver;
                trip.Route = route;
                return trip;
            });

        var result = await CreateService(
            tripRepo.Object,
            busRepo.Object,
            userRepo.Object,
            routeRepo.Object,
            UserRole.Admin,
            1).CreateAsync(request);

        Assert.Equal(driver.Id, result.DriverId);
        Assert.Equal("TRACKER-01", result.DeviceId);
        Assert.Equal(route.Name, result.RouteName);
        Assert.Equal(180, (result.ScheduledArrival - result.ScheduledDeparture).TotalMinutes);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOnlyTripsAssignedToDriver()
    {
        var tripRepo = new Mock<ITripRepository>();
        var assignedBus = CreateBus(1, "Assigned Bus");
        var otherBus = CreateBus(2, "Other Bus");
        tripRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new[]
        {
            CreateTrip(10, assignedBus, 7),
            CreateTrip(11, otherBus, 8)
        });

        var result = (await CreateService(
            tripRepo.Object,
            Mock.Of<IBusRepository>(),
            Mock.Of<IUserRepository>(),
            Mock.Of<IRouteRepository>(),
            UserRole.Driver,
            7).GetAllAsync()).ToList();

        Assert.Equal(10, Assert.Single(result).Id);
    }

    [Fact]
    public async Task UpdateTripStatus_ShouldSetTripToInProgress()
    {
        var tripRepo = new Mock<ITripRepository>();
        var departure = DateTime.UtcNow;
        var trip = CreateTrip(10, CreateBus(1, "Test Bus"), 7);
        trip.DeviceId = "SIM808_01";
        trip.ScheduledDeparture = departure;
        trip.ScheduledArrival = departure.AddHours(3);
        tripRepo.Setup(repo => repo.GetByIdAsync(trip.Id)).ReturnsAsync(trip);
        tripRepo.Setup(repo => repo.GetActiveByDeviceIdAsync(trip.DeviceId)).ReturnsAsync((Trip?)null);
        tripRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Trip>()))
            .ReturnsAsync((Trip updated) => updated);

        var result = await CreateService(
            tripRepo.Object,
            Mock.Of<IBusRepository>(),
            Mock.Of<IUserRepository>(),
            Mock.Of<IRouteRepository>(),
            UserRole.Driver,
            7).UpdateStatusAsync(trip.Id, new UpdateTripStatusDTO
            {
                Status = TripStatus.InProgress,
                ActualDeparture = departure.AddMinutes(1)
            });

        Assert.NotNull(result);
        Assert.Equal(TripStatus.InProgress, result.Status);
    }

    private static TripService CreateService(
        ITripRepository tripRepo,
        IBusRepository busRepo,
        IUserRepository userRepo,
        IRouteRepository routeRepo,
        UserRole role,
        int userId)
    {
        return new TripService(
            tripRepo,
            Mock.Of<IReservationRepository>(),
            Mock.Of<INotificationRepository>(),
            busRepo,
            userRepo,
            routeRepo,
            TestHelpers.CreateMapper(),
            TestHelpers.CreateHttpContext(userId, role));
    }

    private static Bus CreateBus(int id, string name) => new()
    {
        Id = id,
        Name = name,
        PlateNumber = $"BUS-{id}",
        TotalSeats = 40,
        IsActive = true
    };

    private static Trip CreateTrip(int id, Bus bus, int driverId) => new()
    {
        Id = id,
        BusId = bus.Id,
        Bus = bus,
        DriverId = driverId,
        DeviceId = $"DEVICE-{id}",
        ScheduledDeparture = DateTime.UtcNow.AddDays(1),
        ScheduledArrival = DateTime.UtcNow.AddDays(1).AddHours(3),
        Status = TripStatus.Scheduled
    };
}
