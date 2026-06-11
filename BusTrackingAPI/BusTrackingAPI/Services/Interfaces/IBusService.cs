using BusTrackingAPI.DTOs;

namespace BusTrackingAPI.Services.Interfaces
{
    public interface IBusService
    {
        Task<IEnumerable<BusDTO>> GetAllBusesAsync();
        Task<BusDTO?> GetBusByIdAsync(int id);

        Task<BusDTO> CreateBusAsync(CreateBusDTO dto);
        Task<BusDTO?> UpdateBusAsync(int id, UpdateBusDTO dto);

        Task<bool> DeleteBusAsync(int id);
    }
}