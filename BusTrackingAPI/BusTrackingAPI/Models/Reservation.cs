using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTrackingAPI.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? TripId { get; set; }

        [Required]
        [Range(1, 100)]
        public int SeatNumber { get; set; }

        public bool IsVerified { get; set; } = false;

        public string? QrCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("TripId")]
        public Trip? Trip { get; set; }

        public string PickupPlaceName { get; set; } = string.Empty;

        public double PickupLatitude { get; set; }

        public double PickupLongitude { get; set; }
    }
}
