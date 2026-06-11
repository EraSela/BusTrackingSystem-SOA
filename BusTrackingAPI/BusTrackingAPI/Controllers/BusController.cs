using BusTrackingAPI.DTOs;
using BusTrackingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTrackingAPI.Controllers
{
    [ApiController]
    [Route("api/buses")]
    [Authorize]
    public class BusController : ControllerBase
    {
        private readonly IBusService _service;

        public BusController(IBusService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Passenger,Driver,Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllBusesAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Passenger,Driver,Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetBusByIdAsync(id);

            if (result == null)
                return NotFound(new
                {
                    status = 404,
                    error = "Not Found",
                    message = $"Bus with ID {id} was not found."
                });

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateBusDTO dto)
        {
            var result = await _service.CreateBusAsync(dto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, UpdateBusDTO dto)
        {
            var result = await _service.UpdateBusAsync(id, dto);

            if (result == null)
                return NotFound(new
                {
                    status = 404,
                    error = "Not Found",
                    message = $"Bus with ID {id} was not found."
                });

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteBusAsync(id);

            if (!result)
                return NotFound(new
                {
                    status = 404,
                    error = "Not Found",
                    message = $"Bus with ID {id} was not found."
                });

            return Ok(new { message = "Deleted successfully" });
        }
    }
}