using System.ComponentModel.DataAnnotations;
using BusTrackingAPI.Models;

namespace BusTrackingAPI.DTOs
{
    public class TripDTO
    {
        public int Id { get; set; }
        public int BusId { get; set; }
        public int? DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public int? RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public int ExpectedDurationMinutes { get; set; }
        public string? DeviceId { get; set; }
        public string BusName { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int ReservationCount { get; set; }

        public DateTime ScheduledDeparture { get; set; }
        public DateTime? ActualDeparture { get; set; }

        public DateTime ScheduledArrival { get; set; }
        public DateTime? ActualArrival { get; set; }

        public TripStatus Status { get; set; }

        public int DelayMinutes { get; set; }
        public string DelayMessage { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class CreateTripDTO
    {
        [Required]
        public int BusId { get; set; }

        [Range(1, int.MaxValue)]
        public int DriverId { get; set; }

        [Range(1, int.MaxValue)]
        public int RouteId { get; set; }

        [Required, MaxLength(50)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledDeparture { get; set; }
    }

    public class UpdateTripStatusDTO
    {
        [Required]
        public TripStatus Status { get; set; }

        public DateTime? ActualDeparture { get; set; }
        public DateTime? ActualArrival { get; set; }
    }

    public class AssignTripDriverDTO
    {
        [Range(1, int.MaxValue)]
        public int DriverId { get; set; }
    }
}
