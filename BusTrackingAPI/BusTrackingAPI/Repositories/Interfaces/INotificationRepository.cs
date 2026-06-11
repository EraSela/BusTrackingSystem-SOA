using BusTrackingAPI.Models;

namespace BusTrackingAPI.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<bool> ExistsAsync(int tripId,int userId,int? reservationId,NotificationType type);

        Task<Notification> CreateAsync(Notification notification);

        Task<IEnumerable<Notification>> GetAllAsync();

        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
        Task<bool> MarkAsReadAsync(int id, int userId);
    }
}