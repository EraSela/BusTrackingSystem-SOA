using AutoMapper;
using BusTrackingAPI.DTOs;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using BusTrackingAPI.Services.Interfaces;

namespace BusTrackingAPI.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            return _mapper.Map<UserDTO?>(user);
        }

        public async Task<UserDTO> CreateUserAsync(AdminCreateUserDTO dto)
        {
            var existing = await _repo.GetByEmailAsync(dto.Email);
            if (existing != null)
                throw new InvalidOperationException("Email already exists.");

            if (!Enum.IsDefined(typeof(UserRole), dto.Role))
                throw new InvalidOperationException("Invalid user role.");

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                Role = dto.Role,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repo.CreateAsync(user);
            return _mapper.Map<UserDTO>(created);
        }

        public async Task<UserDTO?> UpdateUserAsync(int id, UpdateUserDTO dto)
        {
            var user = await _repo.GetByIdAsync(id);
            if (user == null)
                return null;

            if (dto.Email != null)
            {
                var email = dto.Email.Trim();
                var existing = await _repo.GetByEmailAsync(email);

                if (existing != null && existing.Id != id)
                    throw new InvalidOperationException("Email already exists.");

                user.Email = email;
            }

            if (dto.FullName != null)
                user.FullName = dto.FullName.Trim();

            if (dto.PhoneNumber != null)
                user.PhoneNumber = dto.PhoneNumber.Trim();

            if (dto.Role.HasValue)
            {
                if (!Enum.IsDefined(typeof(UserRole), dto.Role.Value))
                    throw new InvalidOperationException("Invalid user role.");

                user.Role = dto.Role.Value;
            }

            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;

            var updated = await _repo.UpdateAsync(user);
            return _mapper.Map<UserDTO>(updated);
        }

        public async Task<bool> DeleteUserAsync(int id)
            => await _repo.DeleteAsync(id);
    }
}
