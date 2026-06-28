using AutoMapper;
using BusTrackingAPI.DTOs;
using BusTrackingAPI.Helpers;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using BusTrackingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Claims;

namespace BusTrackingAPI.Services.Implementations
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _repo;
        private readonly IBusLocationRepository _locationRepo;
        private readonly ITripRepository _tripRepo;
        private readonly INotificationRepository _notificationRepo;
        private readonly IRecurringTripScheduleService _recurringSchedule;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReservationService(
            IReservationRepository repo,
            IBusRepository busRepo,
            IBusLocationRepository locationRepo,
            ITripRepository tripRepo,
            INotificationRepository notificationRepo,
            IRecurringTripScheduleService recurringSchedule,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _locationRepo = locationRepo;
            _tripRepo = tripRepo;
            _notificationRepo = notificationRepo;
            _recurringSchedule = recurringSchedule;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ReservationDTO>> GetAllAsync()
        {
            return _mapper.Map<IEnumerable<ReservationDTO>>(await _repo.GetAllAsync());
        }

        public async Task<IEnumerable<ReservationDTO>> GetByTripIdAsync(int tripId)
        {
            var trip = await _tripRepo.GetByIdAsync(tripId);
            if (trip == null)
                throw new KeyNotFoundException("Trip was not found.");

            EnsureDriverCanManage(trip);

            return _mapper.Map<IEnumerable<ReservationDTO>>(
                await _repo.GetByTripIdAsync(tripId));
        }

        public async Task<ReservationDTO?> GetByIdAsync(int id)
        {
            var reservation = await _repo.GetByIdAsync(id);
            if (reservation == null)
                return null;

            if (GetCurrentUserRole() != UserRole.Admin.ToString() &&
                reservation.UserId != GetCurrentUserId())
                throw new UnauthorizedAccessException("You are not allowed to view this reservation.");

            return _mapper.Map<ReservationDTO>(reservation);
        }

        public async Task<ReservationDTO> CreateAsync(CreateReservationDTO dto)
        {
            Trip? trip;
            if (dto.TripId.HasValue)
            {
                trip = await _tripRepo.GetByIdAsync(dto.TripId.Value);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.ScheduleId) ||
                    !dto.TravelDate.HasValue)
                    throw new InvalidOperationException(
                        "Select a timetable departure and travel date.");

                trip = await _recurringSchedule.GetOrCreateTripAsync(
                    dto.ScheduleId,
                    dto.TravelDate.Value);
            }

            if (trip == null)
                throw new InvalidOperationException("Selected trip does not exist.");

            if (trip.Status is TripStatus.Completed or TripStatus.Cancelled)
                throw new InvalidOperationException("This trip is not available for reservations.");

            if (trip.ScheduledDeparture <= DateTime.UtcNow)
                throw new InvalidOperationException("Reservations must be made before the trip departs.");

            if (!trip.Bus.IsActive)
                throw new InvalidOperationException("The assigned bus is not available.");

            if (dto.SeatNumber > trip.Bus.TotalSeats)
                throw new InvalidOperationException(
                    $"Seat number must be between 1 and {trip.Bus.TotalSeats}.");

            if (await _repo.IsSeatTakenAsync(trip.Id, dto.SeatNumber))
                throw new InvalidOperationException("This seat is already taken.");

            var reservation = _mapper.Map<Reservation>(dto);
            reservation.UserId = GetCurrentUserId();
            reservation.TripId = trip.Id;
            reservation.QrCode = Guid.NewGuid().ToString();
            reservation.IsVerified = false;
            reservation.CreatedAt = DateTime.UtcNow;

            try
            {
                var created = await _repo.CreateAsync(reservation);
                await CheckIfBusAlreadyReachedPickupAsync(created);
                return _mapper.Map<ReservationDTO>(created);
            }
            catch (DbUpdateException ex) when (
                ex.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation
                })
            {
                throw new InvalidOperationException("This seat is already taken.");
            }
        }

        public Task<IReadOnlyList<TimetableOptionDTO>> GetTimetableAsync()
        {
            return Task.FromResult(_recurringSchedule.GetTimetable());
        }

        public async Task<IEnumerable<ReservationDTO>> GetMyReservationsAsync()
        {
            return _mapper.Map<IEnumerable<ReservationDTO>>(
                await _repo.GetByUserIdAsync(GetCurrentUserId()));
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var reservation = await _repo.GetByIdAsync(id);
            if (reservation == null)
                return false;

            if (GetCurrentUserRole() != UserRole.Admin.ToString() &&
                reservation.UserId != GetCurrentUserId())
                throw new UnauthorizedAccessException("You are not allowed to delete this reservation.");

            return await _repo.DeleteAsync(id);
        }

        public async Task<bool> VerifyQrAsync(string qrCode)
        {
            var reservation = await GetReservationForActiveTripAsync(qrCode);

            if (reservation.IsVerified)
                throw new InvalidOperationException("This reservation has already been checked in.");

            EnsureDriverCanManage(reservation.Trip!);

            if (!await _repo.VerifyByQrCodeAsync(qrCode))
                throw new InvalidOperationException("This reservation has already been checked in.");

            return true;
        }

        public async Task<ReservationDTO?> GetByQrCodeAsync(string qrCode)
        {
            var reservation = await _repo.GetByQrCodeAsync(qrCode);
            if (reservation == null)
                return null;

            if (reservation.Trip == null ||
                reservation.Trip.Status is not (TripStatus.InProgress or TripStatus.Delayed))
                throw new InvalidOperationException(
                    "This reservation does not belong to an active trip.");

            EnsureDriverCanManage(reservation.Trip);
            return _mapper.Map<ReservationDTO>(reservation);
        }

        private async Task<Reservation> GetReservationForActiveTripAsync(string qrCode)
        {
            var reservation = await _repo.GetByQrCodeAsync(qrCode);
            if (reservation == null)
                throw new InvalidOperationException("Invalid QR code.");

            if (reservation.Trip == null ||
                reservation.Trip.Status is not (TripStatus.InProgress or TripStatus.Delayed))
                throw new InvalidOperationException(
                    "This reservation does not belong to an active trip.");

            return reservation;
        }

        private async Task CheckIfBusAlreadyReachedPickupAsync(Reservation reservation)
        {
            if (reservation.Trip == null ||
                reservation.Trip.Status is not (TripStatus.InProgress or TripStatus.Delayed))
                return;

            var latest = await _locationRepo.GetLatestByTripIdAsync(reservation.Trip.Id);
            if (latest == null || latest.Timestamp < DateTime.UtcNow.AddMinutes(-5))
                return;

            var distanceKm = GeoHelper.CalculateDistanceKm(
                latest.Latitude,
                latest.Longitude,
                reservation.PickupLatitude,
                reservation.PickupLongitude);

            if (distanceKm > 0.5)
                return;

            if (!await _notificationRepo.ExistsAsync(
                    reservation.Trip.Id,
                    reservation.UserId,
                    reservation.Id,
                    NotificationType.PickupAlreadyPassed))
            {
                await _notificationRepo.CreateAsync(new Notification
                {
                    TripId = reservation.Trip.Id,
                    UserId = reservation.UserId,
                    ReservationId = reservation.Id,
                    Type = NotificationType.PickupAlreadyPassed,
                    Message = $"Warning: the bus may have already reached or passed your pickup place: {reservation.PickupPlaceName}.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        private int GetCurrentUserId()
        {
            var value = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(value, out var userId))
                throw new UnauthorizedAccessException("User not authenticated.");

            return userId;
        }

        private string GetCurrentUserRole()
        {
            return _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        private void EnsureDriverCanManage(Trip trip)
        {
            if (GetCurrentUserRole() == UserRole.Admin.ToString())
                return;

            if (trip.DriverId != GetCurrentUserId())
                throw new UnauthorizedAccessException("You are not assigned to this trip.");
        }
    }
}
