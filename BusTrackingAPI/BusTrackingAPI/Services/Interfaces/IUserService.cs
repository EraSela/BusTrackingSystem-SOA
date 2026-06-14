using BusTrackingAPI.DTOs;

namespace BusTrackingAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO?> GetUserByIdAsync(int id);
        Task<UserDTO> CreateUserAsync(AdminCreateUserDTO dto);
        Task<UserDTO?> UpdateUserAsync(int id, UpdateUserDTO dto);
        Task<bool> DeleteUserAsync(int id);
    }
}
