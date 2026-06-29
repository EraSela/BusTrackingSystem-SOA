namespace BusTrackingAPI.Helpers
{
    public static class GeoHelper
    {
        public static double CalculateDistanceKm(
            double lat1,
            double lon1,
            double lat2,
            double lon2)
        {
            const double earthRadiusKm = 6371;

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) *
                Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) *
                Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusKm * c;
        }

        public static int EstimateMinutesAway(double distanceKm, double speedKmh)
        {
            if (speedKmh <= 0)
                speedKmh = 40;

            return (int)Math.Ceiling((distanceKm / speedKmh) * 60);
        }

        public static int EstimateRoadMinutesAway(double straightLineDistanceKm, double? currentSpeedKmh)
        {
            const double roadDistanceFactor = 1.6;
            const double defaultBusSpeedKmh = 45;
            const double minReliableSpeedKmh = 30;
            const double maxReliableSpeedKmh = 65;

            var speedKmh = currentSpeedKmh.HasValue && currentSpeedKmh.Value >= minReliableSpeedKmh
                ? Math.Min(currentSpeedKmh.Value, maxReliableSpeedKmh)
                : defaultBusSpeedKmh;

            return EstimateMinutesAway(straightLineDistanceKm * roadDistanceFactor, speedKmh);
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
