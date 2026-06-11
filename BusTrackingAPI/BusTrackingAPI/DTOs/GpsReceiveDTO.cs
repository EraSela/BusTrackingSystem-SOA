using System.ComponentModel.DataAnnotations;

namespace BusTrackingAPI.DTOs
{
    public class GpsReceiveDTO
    {
        [Required]
        public string DeviceId { get; set; } = string.Empty;

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
}