namespace BusTrackingAPI.DTOs
{
    public class EtaDTO
    {
        public int BusId { get; set; }
        public string BusName { get; set; } = string.Empty;

        public int ReservationId { get; set; }
        public string PickupPlaceName { get; set; } = string.Empty;

        public double DistanceKm { get; set; }
        public double SpeedKmh { get; set; }
        public int EstimatedMinutes { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}