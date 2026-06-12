using System.ComponentModel.DataAnnotations;

namespace BusTrackingAPI.DTOs
{
    public class ReservationDTO
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;

        public int TripId { get; set; }
        public int BusId { get; set; }
        public string BusName { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public DateTime ScheduledDeparture { get; set; }
        public DateTime ScheduledArrival { get; set; }

        public int SeatNumber { get; set; }

        public bool IsVerified { get; set; }

        public string? QrCode { get; set; }

        public DateTime CreatedAt { get; set; }

        public string PickupPlaceName { get; set; } = string.Empty;

        public double PickupLatitude { get; set; }

        public double PickupLongitude { get; set; }
    }

    public class CreateReservationDTO
    {
        [Range(1, int.MaxValue)]
        public int? TripId { get; set; }

        public string? ScheduleId { get; set; }

        public DateOnly? TravelDate { get; set; }

        [Required]
        [Range(1, 100)]
        public int SeatNumber { get; set; }

        [Required]
        public string PickupPlaceName { get; set; } = string.Empty;

        [Required]
        public double PickupLatitude { get; set; }

        [Required]
        public double PickupLongitude { get; set; }
    }

    public class TimetableOptionDTO
    {
        public string Id { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string DepartureTime { get; set; } = string.Empty;
        public int ExpectedDurationMinutes { get; set; }
        public int TotalSeats { get; set; }
        public int[] AvailableDays { get; set; } = Array.Empty<int>();
    }

    public class VerifyReservationDTO
    {
        [Required]
        public string QrCode { get; set; } = string.Empty;
    }
}
