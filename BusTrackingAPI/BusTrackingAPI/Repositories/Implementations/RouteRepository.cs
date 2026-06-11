using BusTrackingAPI.Data;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTrackingAPI.Repositories.Implementations
{
    public class RouteRepository : IRouteRepository
    {
        private readonly AppDbContext _context;

        public RouteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BusRoute>> GetAllAsync()
        {
            return await _context.Routes
                .Where(route => route.IsActive)
                .OrderBy(route => route.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<BusRoute?> GetByIdAsync(int id)
        {
            return await _context.Routes.FindAsync(id);
        }
    }
}
