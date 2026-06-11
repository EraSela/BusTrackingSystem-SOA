using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusTrackingAPI.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }

        public int BusId { get; set; }

        public int? DriverId { get; set; }

        public int? RouteId { get; set; }

        [MaxLength(50)]
        public string? DeviceId { get; set; }

        public DateTime ScheduledDeparture { get; set; }
        public DateTime? ActualDeparture { get; set; }

        public DateTime ScheduledArrival { get; set; }
        public DateTime? ActualArrival { get; set; }

        public TripStatus Status { get; set; } = TripStatus.Scheduled;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public int DelayMinutes
        {
            get
            {
                if (!ActualDeparture.HasValue)
                    return 0;

                return (int)(ActualDeparture.Value - ScheduledDeparture).TotalMinutes;
            }
        }

        [NotMapped]
        public string DelayMessage
        {
            get
            {
                if (!ActualDeparture.HasValue)
                    return "Bus has not departed yet.";

                if (DelayMinutes <= 0)
                    return $"Bus left on time at {ActualDeparture.Value:HH:mm}.";

                return $"Bus left at {ActualDeparture.Value:HH:mm}, {DelayMinutes} minutes later than usual.";
            }
        }

        [ForeignKey("BusId")]
        public Bus Bus { get; set; } = null!;

        [ForeignKey("DriverId")]
        public User? Driver { get; set; }

        [ForeignKey("RouteId")]
        public BusRoute? Route { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<BusLocation> Locations { get; set; } = new List<BusLocation>();
    }

    public enum TripStatus
    {
        Scheduled,
        Delayed,
        InProgress,
        Completed,
        Cancelled
    }
}
