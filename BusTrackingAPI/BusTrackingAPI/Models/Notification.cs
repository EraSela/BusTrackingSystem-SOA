using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTrackingAPI.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int TripId { get; set; }

        public int UserId { get; set; }

        public int? ReservationId { get; set; }

        public NotificationType Type { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("TripId")]
        public Trip Trip { get; set; } = null!;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("ReservationId")]
        public Reservation? Reservation { get; set; }
    }

    public enum NotificationType
    {
        Delay,
        PickupTenMinutesAway,
        PickupAlreadyPassed
    }
}