using BusTrackingAPI.Data;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTrackingAPI.Repositories.Implementations
{
    public class TripRepository : ITripRepository
    {
        private readonly AppDbContext _context;

        public TripRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Trip>> GetAllAsync()
        {
            return await _context.Trips
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .Include(t => t.Route)
                .OrderByDescending(t => t.ScheduledDeparture)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Trip?> GetByIdAsync(int id)
        {
            return await _context.Trips
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .Include(t => t.Route)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Trip>> GetByBusIdAsync(int busId)
        {
            return await _context.Trips
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .Include(t => t.Route)
                .Where(t => t.BusId == busId)
                .OrderByDescending(t => t.ScheduledDeparture)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Trip>> GetCompletedTripsAsync()
        {
            return await _context.Trips
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .Include(t => t.Route)
                .Where(t => t.Status == TripStatus.Completed)
                .OrderByDescending(t => t.ScheduledDeparture)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Trip?> GetActiveByBusIdAsync(int busId)
        {
            return await _context.Trips
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .Include(t => t.Route)
                .Where(t =>
                    t.BusId == busId &&
                    (
                        t.Status == TripStatus.InProgress ||
                        t.Status == TripStatus.Delayed
                    ))
                .OrderByDescending(t => t.ScheduledDeparture)
                .FirstOrDefaultAsync();
        }

        public async Task<Trip?> GetActiveByDeviceIdAsync(string deviceId)
        {
            return await _context.Trips
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .Include(t => t.Route)
                .Where(t =>
                    t.DeviceId == deviceId &&
                    (
                        t.Status == TripStatus.InProgress ||
                        t.Status == TripStatus.Delayed
                    ))
                .OrderByDescending(t => t.ScheduledDeparture)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasOverlappingDriverTripAsync(
            int driverId,
            DateTime scheduledDeparture,
            DateTime scheduledArrival)
        {
            return await _context.Trips.AnyAsync(t =>
                t.DriverId == driverId &&
                t.Status != TripStatus.Cancelled &&
                scheduledDeparture < t.ScheduledArrival &&
                scheduledArrival > t.ScheduledDeparture);
        }

        public async Task<bool> HasOverlappingBusTripAsync(
            int busId,
            DateTime scheduledDeparture,
            DateTime scheduledArrival)
        {
            return await _context.Trips.AnyAsync(t =>
                t.BusId == busId &&
                t.Status != TripStatus.Cancelled &&
                scheduledDeparture < t.ScheduledArrival &&
                scheduledArrival > t.ScheduledDeparture);
        }

        public async Task<bool> HasOverlappingDeviceTripAsync(
            string deviceId,
            DateTime scheduledDeparture,
            DateTime scheduledArrival)
        {
            return await _context.Trips.AnyAsync(t =>
                t.DeviceId == deviceId &&
                t.Status != TripStatus.Cancelled &&
                scheduledDeparture < t.ScheduledArrival &&
                scheduledArrival > t.ScheduledDeparture);
        }

        public async Task<Trip> CreateAsync(Trip trip)
        {
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            return await _context.Trips
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .Include(t => t.Route)
                .FirstAsync(t => t.Id == trip.Id);
        }

        public async Task<Trip> UpdateAsync(Trip trip)
        {
            _context.Trips.Update(trip);
            await _context.SaveChangesAsync();

            return await _context.Trips
                .Include(t => t.Bus)
                .Include(t => t.Driver)
                .Include(t => t.Route)
                .FirstAsync(t => t.Id == trip.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null) return false;

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();
            return true;
        }


    }
}
