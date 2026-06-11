using BusTrackingAPI.DTOs;

namespace BusTrackingAPI.Services.Interfaces
{
    public interface IBusLocationService
    {
        Task<IEnumerable<BusLocationDTO>> GetLocationsByBusIdAsync(int busId);

        Task<BusLocationDTO?> GetLatestLocationAsync(int busId);

        Task<IEnumerable<LiveLocationDTO>> GetAllLiveLocationsAsync();

        Task<BusLocationDTO?> AddLocationAsync(GpsReceiveDTO dto);

        Task<IEnumerable<BusLocationDTO>> GetLocationsByDateRangeAsync(
            int busId,
            DateTime from,
            DateTime to);

        Task<bool> DeleteOldLocationsAsync(int daysOld);

        Task<EtaDTO?> GetEtaToPickupAsync(int reservationId);
    }
}