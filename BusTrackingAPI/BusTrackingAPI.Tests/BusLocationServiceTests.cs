using BusTrackingAPI.DTOs;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using BusTrackingAPI.Services.Implementations;
using Moq;

namespace BusTrackingAPI.Tests;

public class BusLocationServiceTests
{
    [Fact]
    public async Task ProcessGps_ShouldSaveLocation_WhenActiveTripExists()
    {
        var locationRepo = new Mock<IBusLocationRepository>();
        var tripRepo = new Mock<ITripRepository>();
        var bus = new Bus
        {
            Id = 1,
            Name = "Test Bus",
            PlateNumber = "TEST-01"
        };
        var activeTrip = new Trip
        {
            Id = 1,
            BusId = bus.Id,
            Bus = bus,
            DeviceId = "SIM808_01",
            Status = TripStatus.InProgress,
            ScheduledDeparture = DateTime.UtcNow,
            ScheduledArrival = DateTime.UtcNow.AddHours(3)
        };
        var dto = new GpsReceiveDTO
        {
            DeviceId = "SIM808_01",
            Latitude = 41.18,
            Longitude = 20.68,
            Speed = 40
        };

        tripRepo.Setup(repo => repo.GetActiveByDeviceIdAsync(dto.DeviceId))
            .ReturnsAsync(activeTrip);
        tripRepo.Setup(repo => repo.GetActiveByBusIdAsync(bus.Id))
            .ReturnsAsync(activeTrip);
        locationRepo.Setup(repo => repo.CreateAsync(It.IsAny<BusLocation>()))
            .ReturnsAsync((BusLocation location) =>
            {
                location.Id = 5;
                location.Bus = bus;
                location.Trip = activeTrip;
                return location;
            });

        var service = CreateService(locationRepo, tripRepo);

        var result = await service.AddLocationAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(bus.Id, result.BusId);
        Assert.Equal(dto.Latitude, result.Latitude);
        locationRepo.Verify(
            repo => repo.CreateAsync(It.IsAny<BusLocation>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessGps_ShouldReturnNull_WhenNoActiveTripExists()
    {
        var locationRepo = new Mock<IBusLocationRepository>();
        var tripRepo = new Mock<ITripRepository>();
        var dto = new GpsReceiveDTO
        {
            DeviceId = "SIM808_01",
            Latitude = 41.18,
            Longitude = 20.68
        };

        tripRepo.Setup(repo => repo.GetActiveByDeviceIdAsync(dto.DeviceId))
            .ReturnsAsync((Trip?)null);

        var service = CreateService(locationRepo, tripRepo);

        var result = await service.AddLocationAsync(dto);

        Assert.Null(result);
        locationRepo.Verify(
            repo => repo.CreateAsync(It.IsAny<BusLocation>()),
            Times.Never);
    }

    private static BusLocationService CreateService(
        Mock<IBusLocationRepository> locationRepo,
        Mock<ITripRepository> tripRepo)
    {
        var reservationRepo = new Mock<IReservationRepository>();
        reservationRepo.Setup(repo =>
                repo.GetByTripIdAsync(It.IsAny<int>()))
            .ReturnsAsync(Array.Empty<Reservation>());

        return new BusLocationService(
            locationRepo.Object,
            Mock.Of<IBusRepository>(),
            tripRepo.Object,
            reservationRepo.Object,
            Mock.Of<INotificationRepository>(),
            TestHelpers.CreateMapper(),
            TestHelpers.CreateHttpContext());
    }
}
