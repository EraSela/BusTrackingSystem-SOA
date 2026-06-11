using BusTrackingAPI.DTOs;

namespace BusTrackingAPI.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDTO>> GetAllAsync();
        Task<IEnumerable<NotificationDTO>> GetMyNotificationsAsync();

        Task<bool> MarkAsReadAsync(int id);
    }
}