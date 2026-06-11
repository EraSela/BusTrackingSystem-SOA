using BusTrackingAPI.Data;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTrackingAPI.Repositories.Implementations
{
    public class BusRepository : IBusRepository
    {
        private readonly AppDbContext _context;

        public BusRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Bus>> GetAllAsync()
        {
            return await _context.Buses
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Bus?> GetByIdAsync(int id)
        {
            return await _context.Buses
                .Include(b => b.Trips)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Bus> CreateAsync(Bus bus)
        {
            _context.Buses.Add(bus);
            await _context.SaveChangesAsync();
            return bus;
        }

        public async Task<Bus> UpdateAsync(Bus bus)
        {
            _context.Buses.Update(bus);
            await _context.SaveChangesAsync();
            return bus;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var bus = await _context.Buses.FindAsync(id);
            if (bus == null) return false;

            _context.Buses.Remove(bus);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Buses.AnyAsync(b => b.Id == id);
        }

    }
}
