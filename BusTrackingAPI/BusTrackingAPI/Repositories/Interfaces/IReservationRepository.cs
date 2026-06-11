using BusTrackingAPI.Models;

namespace BusTrackingAPI.Repositories.Interfaces
{
    public interface IReservationRepository
    {
        Task<IEnumerable<Reservation>> GetAllAsync();

        Task<Reservation?> GetByIdAsync(int id);

        Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId);

        Task<IEnumerable<Reservation>> GetByTripIdAsync(int tripId);
        Task<bool> AnyByBusIdAsync(int busId);

        Task<Reservation> CreateAsync(Reservation reservation);

        Task<Reservation> UpdateAsync(Reservation reservation);

        Task<bool> DeleteAsync(int id);

        Task<bool> IsSeatTakenAsync(int tripId, int seatNumber);

        Task<int> GetAvailableSeatsCountAsync(int tripId);

        Task<bool> VerifyByQrCodeAsync(string qrCode);

        Task<Reservation?> GetByQrCodeAsync(string qrCode);

    }
}
