using System.ComponentModel.DataAnnotations;

namespace BusTrackingAPI.Models
{
    public class BusRoute
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Origin { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Destination { get; set; } = string.Empty;

        [Range(1, 1440)]
        public int ExpectedDurationMinutes { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
