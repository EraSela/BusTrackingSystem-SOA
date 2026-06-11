using System.ComponentModel.DataAnnotations;

namespace BusTrackingAPI.Models
{
    public class Bus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PlateNumber { get; set; } = string.Empty;

        public TimeSpan DepartureTime { get; set; }

        [Range(1, 100)]
        public int TotalSeats { get; set; } = 40;

        public bool IsActive { get; set; } = true;

        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<BusLocation> Locations { get; set; } = new List<BusLocation>();
    }
}
