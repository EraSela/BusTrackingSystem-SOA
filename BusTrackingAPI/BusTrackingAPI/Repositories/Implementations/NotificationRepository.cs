using BusTrackingAPI.Data;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTrackingAPI.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(
            int tripId,
            int userId,
            int? reservationId,
            NotificationType type)
        {
            return await _context.Notifications.AnyAsync(n =>
                n.TripId == tripId &&
                n.UserId == userId &&
                n.ReservationId == reservationId &&
                n.Type == type);
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await _context.Notifications
                .Include(n => n.Trip)
                .Include(n => n.User)
                .Include(n => n.Reservation)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(int id, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
                return false;

            notification.IsRead = true;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}