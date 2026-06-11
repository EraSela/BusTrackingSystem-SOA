using System.ComponentModel.DataAnnotations;

namespace BusTrackingAPI.DTOs
{
    public class BusLocationDTO
    {
        public int Id { get; set; }
        public int BusId { get; set; }
        public int? TripId { get; set; }
        public string BusName { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public double? Speed { get; set; }
        public double? Heading { get; set; }
        public double? Accuracy { get; set; }

        public string? Status { get; set; }
        public int? Signal { get; set; }

        public DateTime Timestamp { get; set; }
    }

    public class CreateBusLocationDTO
    {
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

        public string? Status { get; set; }
        public int? Signal { get; set; }
    }

    
    public class LiveLocationDTO
    {
        public int BusId { get; set; }
        public int? TripId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string BusName { get; set; } = string.Empty;

        public TimeSpan DepartureTime { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public double? Speed { get; set; }
        public double? Heading { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
