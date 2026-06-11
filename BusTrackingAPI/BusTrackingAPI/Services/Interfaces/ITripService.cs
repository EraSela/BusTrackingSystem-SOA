using BusTrackingAPI.DTOs;

namespace BusTrackingAPI.Services.Interfaces
{
    public interface ITripService
    {
        Task<IEnumerable<TripDTO>> GetAllAsync();
        Task<TripDTO?> GetByIdAsync(int id);
        Task<IEnumerable<TripDTO>> GetByBusIdAsync(int busId);
        Task<IEnumerable<TripDTO>> GetCompletedTripsAsync();
        Task<TripDTO?> GetActiveByBusIdAsync(int busId);
        Task<TripDTO> CreateAsync(CreateTripDTO dto);
        Task<TripDTO?> UpdateStatusAsync(int id, UpdateTripStatusDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}