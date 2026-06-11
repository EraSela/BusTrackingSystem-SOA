using BusTrackingAPI.DTOs;

namespace BusTrackingAPI.Services.Interfaces
{
    public interface IReservationService
    {
        Task<IEnumerable<ReservationDTO>> GetAllAsync();
        Task<IEnumerable<ReservationDTO>> GetMyReservationsAsync();

        Task<ReservationDTO?> GetByIdAsync(int id);

        Task<ReservationDTO> CreateAsync(CreateReservationDTO dto);

        Task<bool> DeleteAsync(int id);

        Task<bool> VerifyQrAsync(string qrCode);
        Task<ReservationDTO?> GetByQrCodeAsync(string qrCode);
    }
}