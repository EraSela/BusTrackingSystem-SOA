using BusTrackingAPI.Models;

namespace BusTrackingAPI.Repositories.Interfaces
{
    public interface IBusLocationRepository
    {
        Task<IEnumerable<BusLocation>> GetAllByBusIdAsync(int busId);
        Task<BusLocation?> GetLatestByBusIdAsync(int busId);
        Task<BusLocation?> GetLatestByTripIdAsync(int tripId);
        Task<IEnumerable<BusLocation>> GetAllLiveLocationsAsync();
        Task<BusLocation> CreateAsync(BusLocation location);
        Task<IEnumerable<BusLocation>> GetByDateRangeAsync(int busId, DateTime from, DateTime to);
        Task<bool> DeleteOldLocationsAsync(int daysOld);
    }
}
