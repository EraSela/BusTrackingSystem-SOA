using System.ComponentModel.DataAnnotations;

namespace BusTrackingAPI.DTOs
{
    public class RouteDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public int ExpectedDurationMinutes { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateRouteDTO
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Origin { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Destination { get; set; } = string.Empty;

        [Range(1, 1440)]
        public int ExpectedDurationMinutes { get; set; }
    }
}
