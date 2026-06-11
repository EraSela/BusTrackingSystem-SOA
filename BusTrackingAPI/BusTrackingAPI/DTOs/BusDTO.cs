using System.ComponentModel.DataAnnotations;

namespace BusTrackingAPI.DTOs
{
    public class BusDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public TimeSpan DepartureTime { get; set; }
        public int TotalSeats { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateBusDTO
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PlateNumber { get; set; } = string.Empty;


        [Required]
        public TimeSpan DepartureTime { get; set; }

        [Range(1, 100)]
        public int TotalSeats { get; set; } = 40;

    }

    public class UpdateBusDTO
    {
        [MaxLength(50)]
        public string? Name { get; set; }

        [MaxLength(20)]
        public string? PlateNumber { get; set; }

        public TimeSpan? DepartureTime { get; set; }

        [Range(1, 100)]
        public int? TotalSeats { get; set; }

        public bool? IsActive { get; set; }

    }
}
