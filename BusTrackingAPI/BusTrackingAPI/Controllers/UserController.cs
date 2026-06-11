using BusTrackingAPI.Services.Interfaces;
using BusTrackingAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusTrackingAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllUsersAsync());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _service.GetUserByIdAsync(id);

            if (user == null)
                return NotFound(new
                {
                    status = 404,
                    error = "Not Found",
                    message = $"User with ID {id} was not found."
                });

            return Ok(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(AdminCreateUserDTO dto)
        {
            return Ok(await _service.CreateUserAsync(dto));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, UpdateUserDTO dto)
        {
            var user = await _service.UpdateUserAsync(id, dto);

            if (user == null)
                return NotFound(new
                {
                    status = 404,
                    error = "Not Found",
                    message = $"User with ID {id} was not found."
                });

            return Ok(user);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteUserAsync(id);

            if (!result)
                return NotFound(new
                {
                    status = 404,
                    error = "Not Found",
                    message = $"User with ID {id} was not found."
                });

            return Ok(new { message = "User deleted successfully." });
        }
    }
}
