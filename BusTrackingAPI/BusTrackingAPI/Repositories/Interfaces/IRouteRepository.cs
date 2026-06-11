using BusTrackingAPI.Models;

namespace BusTrackingAPI.Repositories.Interfaces
{
    public interface IRouteRepository
    {
        Task<IEnumerable<BusRoute>> GetAllAsync();
        Task<BusRoute?> GetByIdAsync(int id);
    }
}
