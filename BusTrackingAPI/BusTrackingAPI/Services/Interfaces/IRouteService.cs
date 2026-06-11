using BusTrackingAPI.DTOs;

namespace BusTrackingAPI.Services.Interfaces
{
    public interface IRouteService
    {
        Task<IEnumerable<RouteDTO>> GetAllAsync();
    }
}
