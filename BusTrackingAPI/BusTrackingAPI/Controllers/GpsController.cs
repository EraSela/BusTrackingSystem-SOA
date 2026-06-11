using BusTrackingAPI.DTOs;
using BusTrackingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BusTrackingAPI.Controllers
{
    [ApiController]
    [Route("api/gps")]
    public class GpsController : ControllerBase
    {
        private readonly IBusLocationService _service;

        public GpsController(IBusLocationService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpPost("receive")]
        public async Task<IActionResult> Receive([FromBody] GpsReceiveDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { status = 400, error = "Bad Request", message = "Invalid GPS data provided." });

            try
            {
                var result = await _service.AddLocationAsync(dto);

                if (result == null)
                    return BadRequest(new { status = 400, error = "Bad Request", message = $"No active trip found for device ID '{dto.DeviceId}'." });

                return Ok(new
                {
                    message = "GPS received",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = 400,
                    error = "Bad Request",
                    message = ex.Message
                });
            }
        }
    }
}
