using BusTrackingAPI.Data;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTrackingAPI.Repositories.Implementations
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;

        public ReservationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Reservation>> GetAllAsync()
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Trip)!.ThenInclude(t => t.Bus)
                .Include(r => r.Trip)!.ThenInclude(t => t.Route)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Reservation?> GetByIdAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Trip)!.ThenInclude(t => t.Bus)
                .Include(r => r.Trip)!.ThenInclude(t => t.Route)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Trip)!.ThenInclude(t => t.Bus)
                .Include(r => r.Trip)!.ThenInclude(t => t.Route)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Trip!.ScheduledDeparture)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByTripIdAsync(int tripId)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Trip)!.ThenInclude(t => t.Bus)
                .Include(r => r.Trip)!.ThenInclude(t => t.Route)
                .Where(r => r.TripId == tripId)
                .OrderBy(r => r.SeatNumber)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> AnyByBusIdAsync(int busId)
        {
            return await _context.Reservations.AnyAsync(r => r.Trip != null && r.Trip.BusId == busId);
        }

        public async Task<Reservation> CreateAsync(Reservation reservation)
        {
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Trip)!.ThenInclude(t => t.Bus)
                .Include(r => r.Trip)!.ThenInclude(t => t.Route)
                .FirstAsync(r => r.Id == reservation.Id);
        }

        public async Task<Reservation> UpdateAsync(Reservation reservation)
        {
            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null) return false;

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsSeatTakenAsync(int tripId, int seatNumber)
        {
            return await _context.Reservations
                .AnyAsync(r => r.TripId == tripId && r.SeatNumber == seatNumber);
        }

        public async Task<int> GetAvailableSeatsCountAsync(int tripId)
        {
            var trip = await _context.Trips
                .Include(t => t.Bus)
                .FirstOrDefaultAsync(t => t.Id == tripId);
            if (trip == null) return 0;

            var taken = await _context.Reservations
                .CountAsync(r => r.TripId == tripId);

            return trip.Bus.TotalSeats - taken;
        }

        public async Task<bool> VerifyByQrCodeAsync(string qrCode)
        {
            var updated = await _context.Reservations
                .Where(r => r.QrCode == qrCode && !r.IsVerified)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(r => r.IsVerified, true));

            return updated == 1;
        }

        public async Task<Reservation?> GetByQrCodeAsync(string qrCode)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Trip)!.ThenInclude(t => t.Bus)
                .Include(r => r.Trip)!.ThenInclude(t => t.Route)
                .FirstOrDefaultAsync(r => r.QrCode == qrCode);
        }
    }
}
