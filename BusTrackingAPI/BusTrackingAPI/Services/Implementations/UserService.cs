using AutoMapper;
using BusTrackingAPI.DTOs;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using BusTrackingAPI.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BusTrackingAPI.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public UserService(IUserRepository repo, IMapper mapper, IConfiguration config)
        {
            _repo = repo;
            _mapper = mapper;
            _config = config;
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

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
        {
            var exists = await _repo.GetByEmailAsync(dto.Email);
            if (exists != null)
                throw new InvalidOperationException("Email already exists.");

            var user = _mapper.Map<User>(dto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var created = await _repo.CreateAsync(user);

            return new AuthResponseDTO
            {
                Token = GenerateToken(created),
                FullName = created.FullName,
                Email = created.Email,
                Role = created.Role
            };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {
            var user = await _repo.GetByEmailAsync(dto.Email);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            return new AuthResponseDTO
            {
                Token = GenerateToken(user),
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<bool> DeleteUserAsync(int id)
            => await _repo.DeleteAsync(id);

        // JWT
        private string GenerateToken(User user)
        {
            var jwt = _config.GetSection("JwtSettings");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["SecretKey"]!));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
