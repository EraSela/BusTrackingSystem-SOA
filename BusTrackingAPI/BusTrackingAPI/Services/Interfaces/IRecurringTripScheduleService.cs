namespace BusTrackingAPI.Services.Interfaces
{
    public interface IRecurringTripScheduleService
    {
        Task<int> EnsureUpcomingTripsAsync(
            DateTime? utcNow = null,
            int daysAhead = 30);
    }
}
