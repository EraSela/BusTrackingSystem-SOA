using BusTrackingAPI.DTOs;
using BusTrackingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTrackingAPI.Controllers
{
    [ApiController]
    [Route("api/trips")]
    [Authorize]
    public class TripController : ControllerBase
    {
        private readonly ITripService _service;

        public TripController(ITripService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Passenger,Driver,Admin")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Passenger,Driver,Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
                return NotFound(new
                {
                    status = 404,
                    error = "Not Found",
                    message = $"Trip with ID {id} was not found."
                });

            return Ok(result);
        }

        [HttpGet("bus/{busId}")]
        [Authorize(Roles = "Passenger,Driver,Admin")]
        public async Task<IActionResult> GetByBus(int busId)
        {
            return Ok(await _service.GetByBusIdAsync(busId));
        }

        [HttpGet("bus/{busId}/active")]
        [Authorize(Roles = "Passenger,Driver,Admin")]
        public async Task<IActionResult> GetActive(int busId)
        {
            var result = await _service.GetActiveByBusIdAsync(busId);

            if (result == null)
                return NotFound(new
                {
                    status = 404,
                    error = "Not Found",
                    message = $"No active trip found for bus {busId}."
                });

            return Ok(result);
        }

        [HttpGet("completed")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCompleted()
        {
            return Ok(await _service.GetCompletedTripsAsync());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateTripDTO dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Driver,Admin")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateTripStatusDTO dto)
        {
            var result = await _service.UpdateStatusAsync(id, dto);

            if (result == null)
                return NotFound(new
                {
                    status = 404,
                    error = "Not Found",
                    message = $"Trip with ID {id} was not found."
                });

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound(new
                {
                    status = 404,
                    error = "Not Found",
                    message = $"Trip with ID {id} was not found."
                });

            return Ok(new { message = "Trip deleted successfully." });
        }
    }
}