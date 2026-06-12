using BusTrackingAPI.DTOs;
using BusTrackingAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTrackingAPI.Controllers
{
    [ApiController]
    [Route("api/reservations")]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _service;

        public ReservationController(IReservationService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("my")]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> GetMyReservations()
        {
            return Ok(await _service.GetMyReservationsAsync());
        }

        [HttpGet("timetable")]
        [Authorize(Roles = "Passenger,Admin")]
        public async Task<IActionResult> GetTimetable()
        {
            return Ok(await _service.GetTimetableAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var res = await _service.GetByIdAsync(id);

                if (res == null)
                    return NotFound(new { status = 404, error = "Not Found", message = $"Reservation with ID {id} was not found." });

                return Ok(res);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> Create(CreateReservationDTO dto)
        {
            try
            {
                var result = await _service.CreateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = 400, error = "Bad Request", message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);

                if (!result)
                    return NotFound(new { status = 404, error = "Not Found", message = $"Reservation with ID {id} was not found." });

                return Ok(new { message = "Deleted successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("qr/{qrCode}")]
        [Authorize(Roles = "Driver,Admin")]
        public async Task<IActionResult> GetByQrCode(string qrCode)
        {
            var result = await _service.GetByQrCodeAsync(qrCode);

            if (result == null)
                return NotFound(new
                {
                    status = 404,
                    error = "Not Found",
                    message = "Invalid QR code."
                });

            return Ok(result);
        }

        [HttpPost("verify")]
        [Authorize(Roles = "Driver,Admin")]
        public async Task<IActionResult> Verify(VerifyReservationDTO dto)
        {
            try
            {
                var result = await _service.VerifyQrAsync(dto.QrCode);

                if (!result)
                    return NotFound(new
                    {
                        status = 404,
                        error = "Not Found",
                        message = "Invalid QR code."
                    });

                return Ok(new
                {
                    success = true,
                    message = "Passenger checked in successfully."
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
