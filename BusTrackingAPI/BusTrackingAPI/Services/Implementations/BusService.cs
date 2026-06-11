using AutoMapper;
using BusTrackingAPI.DTOs;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using BusTrackingAPI.Services.Interfaces;

namespace BusTrackingAPI.Services.Implementations
{
    public class BusService : IBusService
    {
        private readonly IBusRepository _repo;
        private readonly ITripRepository _tripRepo;
        private readonly IReservationRepository _reservationRepo;
        private readonly IMapper _mapper;

        public BusService(
            IBusRepository repo,
            ITripRepository tripRepo,
            IReservationRepository reservationRepo,
            IMapper mapper)
        {
            _repo = repo;
            _tripRepo = tripRepo;
            _reservationRepo = reservationRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BusDTO>> GetAllBusesAsync()
        {
            var buses = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<BusDTO>>(buses);
        }

        public async Task<BusDTO?> GetBusByIdAsync(int id)
        {
            var bus = await _repo.GetByIdAsync(id);
            return _mapper.Map<BusDTO?>(bus);
        }

        public async Task<BusDTO> CreateBusAsync(CreateBusDTO dto)
        {
            var bus = _mapper.Map<Bus>(dto);
            var created = await _repo.CreateAsync(bus);
            return _mapper.Map<BusDTO>(created);
        }

        public async Task<BusDTO?> UpdateBusAsync(int id, UpdateBusDTO dto)
        {
            var bus = await _repo.GetByIdAsync(id);

            if (bus == null)
                return null;

            if (dto.Name != null)
                bus.Name = dto.Name;

            if (dto.PlateNumber != null)
                bus.PlateNumber = dto.PlateNumber;

            if (dto.TotalSeats.HasValue)
                bus.TotalSeats = dto.TotalSeats.Value;

            if (dto.IsActive.HasValue)
                bus.IsActive = dto.IsActive.Value;

            var updated = await _repo.UpdateAsync(bus);

            return _mapper.Map<BusDTO>(updated);
        }

        public async Task<bool> DeleteBusAsync(int id)
        {
            var bus = await _repo.GetByIdAsync(id);

            if (bus == null)
                return false;

            var trips = await _tripRepo.GetByBusIdAsync(id);

            if (trips.Any())
                throw new Exception("Cannot delete this bus because it has trips.");

            if (await _reservationRepo.AnyByBusIdAsync(id))
                throw new Exception("Cannot delete this bus because it has reservations.");

            return await _repo.DeleteAsync(id);
        }

    }
}
