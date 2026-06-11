using BusTrackingAPI.Controllers;
using BusTrackingAPI.DTOs;
using BusTrackingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BusTrackingAPI.Tests;

public class ControllerTests
{
    [Fact]
    public async Task ReservationController_Create_ShouldReturnOk_WhenReservationIsValid()
    {
        var service = new Mock<IReservationService>();
        var request = new CreateReservationDTO
        {
            TripId = 1,
            SeatNumber = 4,
            PickupPlaceName = "Struga Bus Station",
            PickupLatitude = 41.17799,
            PickupLongitude = 20.67784
        };
        var response = new ReservationDTO
        {
            Id = 10,
            TripId = request.TripId,
            SeatNumber = request.SeatNumber,
        };

        service.Setup(item => item.CreateAsync(request))
            .ReturnsAsync(response);
        var controller = new ReservationController(service.Object);

        var result = await controller.Create(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(response, okResult.Value);
    }

    [Fact]
    public async Task GpsController_Receive_ShouldReturnBadRequest_WhenDeviceHasNoActiveTrip()
    {
        var service = new Mock<IBusLocationService>();
        var request = new GpsReceiveDTO
        {
            DeviceId = "SIM808_01",
            Latitude = 41.18,
            Longitude = 20.68
        };

        service.Setup(item => item.AddLocationAsync(request))
            .ReturnsAsync((BusLocationDTO?)null);
        var controller = new GpsController(service.Object);

        var result = await controller.Receive(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }
}
