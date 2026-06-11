using BusTrackingAPI.Models;

namespace BusTrackingAPI.Repositories.Interfaces
{
    public interface ITripRepository
    {
        Task<IEnumerable<Trip>> GetAllAsync();

        Task<Trip?> GetByIdAsync(int id);

        Task<IEnumerable<Trip>> GetByBusIdAsync(int busId);

        Task<IEnumerable<Trip>> GetCompletedTripsAsync();

        Task<Trip?> GetActiveByBusIdAsync(int busId);

        Task<Trip?> GetActiveByDeviceIdAsync(string deviceId);

        Task<bool> HasOverlappingDriverTripAsync(
            int driverId,
            DateTime scheduledDeparture,
            DateTime scheduledArrival);

        Task<bool> HasOverlappingBusTripAsync(
            int busId,
            DateTime scheduledDeparture,
            DateTime scheduledArrival);

        Task<bool> HasOverlappingDeviceTripAsync(
            string deviceId,
            DateTime scheduledDeparture,
            DateTime scheduledArrival);

        Task<Trip> CreateAsync(Trip trip);

        Task<Trip> UpdateAsync(Trip trip);

        Task<bool> DeleteAsync(int id);
    }
}
