using BusTrackingAPI.DTOs;
using BusTrackingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTrackingAPI.Controllers
{
    [ApiController]
    [Route("api/locations")]
    [Authorize]
    public class BusLocationController : ControllerBase
    {
        private readonly IBusLocationService _service;

        public BusLocationController(IBusLocationService service)
        {
            _service = service;
        }

        [HttpGet("{busId}")]
        [Authorize(Roles = "Driver,Admin")]
        public async Task<IActionResult> GetBusLocations(int busId)
        {
            var result = await _service.GetLocationsByBusIdAsync(busId);
            return Ok(result);
        }

        [HttpGet("latest/{busId}")]
        [Authorize(Roles = "Passenger,Driver,Admin")]
        public async Task<IActionResult> GetLatest(int busId)
        {
            var result = await _service.GetLatestLocationAsync(busId);

            if (result == null)
                return NotFound(new
                {
                    status = 404,
                    error = "Not Found",
                    message = $"No location found for bus {busId}."
                });

            return Ok(result);
        }

        [HttpGet("live")]
        [Authorize(Roles = "Passenger,Driver,Admin")]
        public async Task<IActionResult> GetLive()
        {
            var result = await _service.GetAllLiveLocationsAsync();
            return Ok(result);
        }

        [HttpGet("history/{busId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetHistory(int busId, DateTime from, DateTime to)
        {
            var result = await _service.GetLocationsByDateRangeAsync(busId, from, to);
            return Ok(result);
        }

        [HttpGet("eta/reservation/{reservationId}")]
        [Authorize(Roles = "Passenger,Admin")]
        public async Task<IActionResult> GetEtaToPickup(int reservationId)
        {
            var result = await _service.GetEtaToPickupAsync(reservationId);

            if (result == null)
                return NotFound(new
                {
                    status = 404,
                    error = "Not Found",
                    message = "ETA could not be calculated for this reservation."
                });

            return Ok(result);
        }
    }
}