using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTrackingAPI.Models
{
    public class BusLocation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BusId { get; set; }

        public int? TripId { get; set; }

        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Range(0, 300)]
        public double? Speed { get; set; }

        [Range(0, 360)]
        public double? Heading { get; set; }

        public double? Accuracy { get; set; }

       
        [MaxLength(20)]
        public string? Status { get; set; }

        // SIM signal strength (CSQ)
        public int? Signal { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [ForeignKey("BusId")]
        public Bus Bus { get; set; } = null!;

        [ForeignKey("TripId")]
        public Trip? Trip { get; set; }
    }
}
