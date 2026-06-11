using BusTrackingAPI.Models;

namespace BusTrackingAPI.DTOs
{
    public class NotificationDTO
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int UserId { get; set; }
        public int ReservationId { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}