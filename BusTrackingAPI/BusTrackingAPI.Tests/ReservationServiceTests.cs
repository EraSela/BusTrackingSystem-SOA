using BusTrackingAPI.DTOs;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using BusTrackingAPI.Services.Implementations;
using BusTrackingAPI.Services.Interfaces;
using Moq;

namespace BusTrackingAPI.Tests;

public class ReservationServiceTests
{
    private readonly Mock<IReservationRepository> _reservationRepo = new();
    private readonly Mock<ITripRepository> _tripRepo = new();

    [Fact]
    public async Task CreateReservation_ShouldCreateReservation_WhenSeatIsAvailable()
    {
        var trip = CreateTrip();
        var dto = CreateReservation();
        var tripId = dto.TripId!.Value;
        _tripRepo.Setup(repo => repo.GetByIdAsync(tripId)).ReturnsAsync(trip);
        _reservationRepo.Setup(repo => repo.IsSeatTakenAsync(tripId, dto.SeatNumber)).ReturnsAsync(false);
        _reservationRepo.Setup(repo => repo.CreateAsync(It.IsAny<Reservation>()))
            .ReturnsAsync((Reservation reservation) =>
            {
                reservation.Id = 10;
                reservation.Trip = trip;
                reservation.User = CreateUser();
                return reservation;
            });

        var result = await CreateService().CreateAsync(dto);

        Assert.Equal(10, result.Id);
        Assert.Equal(dto.TripId, result.TripId);
        Assert.False(string.IsNullOrWhiteSpace(result.QrCode));
    }

    [Fact]
    public async Task CreateReservation_ShouldThrowException_WhenSeatIsAlreadyTaken()
    {
        var dto = CreateReservation();
        var tripId = dto.TripId!.Value;
        _tripRepo.Setup(repo => repo.GetByIdAsync(tripId)).ReturnsAsync(CreateTrip());
        _reservationRepo.Setup(repo => repo.IsSeatTakenAsync(tripId, dto.SeatNumber)).ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateAsync(dto));

        Assert.Equal("This seat is already taken.", exception.Message);
    }

    [Fact]
    public async Task VerifyQrCode_ShouldReturnTrue_WhenQrCodeExists()
    {
        const string qrCode = "valid-qr";
        var trip = CreateTrip(TripStatus.InProgress);
        var reservation = new Reservation
        {
            Id = 20,
            UserId = 1,
            User = CreateUser(),
            TripId = trip.Id,
            Trip = trip,
            SeatNumber = 4,
            QrCode = qrCode
        };
        _reservationRepo.Setup(repo => repo.GetByQrCodeAsync(qrCode)).ReturnsAsync(reservation);
        _reservationRepo.Setup(repo => repo.VerifyByQrCodeAsync(qrCode)).ReturnsAsync(true);

        Assert.True(await CreateService(UserRole.Admin).VerifyQrAsync(qrCode));
    }

    private ReservationService CreateService(UserRole role = UserRole.Passenger)
    {
        return new ReservationService(
            _reservationRepo.Object,
            Mock.Of<IBusRepository>(),
            Mock.Of<IBusLocationRepository>(),
            _tripRepo.Object,
            Mock.Of<INotificationRepository>(),
            Mock.Of<IRecurringTripScheduleService>(),
            TestHelpers.CreateMapper(),
            TestHelpers.CreateHttpContext(1, role));
    }

    private static CreateReservationDTO CreateReservation() => new()
    {
        TripId = 1,
        SeatNumber = 4,
        PickupPlaceName = "Struga Bus Station",
        PickupLatitude = 41.17799,
        PickupLongitude = 20.67784
    };

    private static Trip CreateTrip(TripStatus status = TripStatus.Scheduled)
    {
        var bus = new Bus { Id = 1, Name = "Test Bus", PlateNumber = "TEST-01", TotalSeats = 40, IsActive = true };
        return new Trip
        {
            Id = 1,
            BusId = bus.Id,
            Bus = bus,
            DriverId = 1,
            DeviceId = "SIM808_01",
            ScheduledDeparture = DateTime.UtcNow.AddDays(1),
            ScheduledArrival = DateTime.UtcNow.AddDays(1).AddHours(3),
            Status = status
        };
    }

    private static User CreateUser() => new()
    {
        Id = 1,
        FullName = "Test Passenger",
        Email = "passenger@test.local",
        PasswordHash = "hash"
    };
}
