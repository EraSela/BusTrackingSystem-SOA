using BusTrackingAPI.Data;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTrackingAPI.Repositories.Implementations
{
    public class BusLocationRepository : IBusLocationRepository
    {
        private readonly AppDbContext _context;

        public BusLocationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BusLocation>> GetAllByBusIdAsync(int busId)
        {
            return await _context.BusLocations
                .Where(l => l.BusId == busId)
                .OrderByDescending(l => l.Timestamp)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<BusLocation?> GetLatestByBusIdAsync(int busId)
        {
            return await _context.BusLocations
                .Include(l => l.Bus)
                .Include(l => l.Trip)!.ThenInclude(t => t.Route)
                .Where(l => l.BusId == busId)
                .OrderByDescending(l => l.Timestamp)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<BusLocation?> GetLatestByTripIdAsync(int tripId)
        {
            return await _context.BusLocations
                .Include(l => l.Bus)
                .Include(l => l.Trip)!.ThenInclude(t => t.Route)
                .Where(l => l.TripId == tripId)
                .OrderByDescending(l => l.Timestamp)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

      
        public async Task<IEnumerable<BusLocation>> GetAllLiveLocationsAsync()
        {
            var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);

            var latestLocationIds = await _context.BusLocations
                .Where(l => l.Timestamp >= fiveMinutesAgo)
                .Where(l => l.TripId != null)
                .GroupBy(l => l.TripId)
                .Select(g => g.Max(l => l.Id))
                .ToListAsync();

            return await _context.BusLocations
                .Include(l => l.Bus)
                .Include(l => l.Trip)!.ThenInclude(t => t.Route)
                .Where(l => latestLocationIds.Contains(l.Id))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<BusLocation> CreateAsync(BusLocation location)
        {
            _context.BusLocations.Add(location);
            await _context.SaveChangesAsync();

            return await _context.BusLocations
                .Include(l => l.Bus)
                .Include(l => l.Trip)!.ThenInclude(t => t.Route)
                .FirstAsync(l => l.Id == location.Id);
        }

        public async Task<IEnumerable<BusLocation>> GetByDateRangeAsync(int busId, DateTime from, DateTime to)
        {
            return await _context.BusLocations
                .Where(l => l.BusId == busId &&
                            l.Timestamp >= from &&
                            l.Timestamp <= to)
                .OrderBy(l => l.Timestamp)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> DeleteOldLocationsAsync(int daysOld)
        {
            var cutoff = DateTime.UtcNow.AddDays(-daysOld);

            var oldRecords = await _context.BusLocations
                .Where(l => l.Timestamp < cutoff)
                .ToListAsync();

            if (!oldRecords.Any())
                return true;

            _context.BusLocations.RemoveRange(oldRecords);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
