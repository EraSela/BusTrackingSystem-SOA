using BusTrackingAPI.Models;

namespace BusTrackingAPI.Repositories.Interfaces
{
    public interface IBusRepository
    {
        Task<IEnumerable<Bus>> GetAllAsync();
        Task<Bus?> GetByIdAsync(int id);
        Task<Bus> CreateAsync(Bus bus);
        Task<Bus> UpdateAsync(Bus bus);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
