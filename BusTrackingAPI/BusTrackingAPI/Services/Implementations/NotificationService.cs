using AutoMapper;
using BusTrackingAPI.DTOs;
using BusTrackingAPI.Repositories.Interfaces;
using BusTrackingAPI.Services.Interfaces;
using System.Security.Claims;

namespace BusTrackingAPI.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationService(
            INotificationRepository notificationRepo,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _notificationRepo = notificationRepo;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<NotificationDTO>> GetAllAsync()
        {
            var notifications = await _notificationRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<NotificationDTO>>(notifications);
        }

        public async Task<IEnumerable<NotificationDTO>> GetMyNotificationsAsync()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User not authenticated.");

            var userId = int.Parse(userIdClaim);

            var notifications = await _notificationRepo.GetByUserIdAsync(userId);

            return _mapper.Map<IEnumerable<NotificationDTO>>(notifications);
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User not authenticated.");

            var userId = int.Parse(userIdClaim);

            return await _notificationRepo.MarkAsReadAsync(id, userId);
        }
    }
}