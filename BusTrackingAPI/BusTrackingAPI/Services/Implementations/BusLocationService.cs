using AutoMapper;
using BusTrackingAPI.DTOs;
using BusTrackingAPI.Helpers;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using BusTrackingAPI.Services.Interfaces;
using System.Security.Claims;

namespace BusTrackingAPI.Services.Implementations
{
    public class BusLocationService : IBusLocationService
    {
        private readonly IBusLocationRepository _repo;
        private readonly ITripRepository _tripRepo;
        private readonly IReservationRepository _reservationRepo;
        private readonly INotificationRepository _notificationRepo;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BusLocationService(
            IBusLocationRepository repo,
            IBusRepository busRepo,
            ITripRepository tripRepo,
            IReservationRepository reservationRepo,
            INotificationRepository notificationRepo,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _tripRepo = tripRepo;
            _reservationRepo = reservationRepo;
            _notificationRepo = notificationRepo;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<BusLocationDTO>> GetLocationsByBusIdAsync(int busId)
        {
            return _mapper.Map<IEnumerable<BusLocationDTO>>(
                await _repo.GetAllByBusIdAsync(busId));
        }

        public async Task<BusLocationDTO?> GetLatestLocationAsync(int busId)
        {
            return _mapper.Map<BusLocationDTO?>(
                await _repo.GetLatestByBusIdAsync(busId));
        }

        public async Task<IEnumerable<LiveLocationDTO>> GetAllLiveLocationsAsync()
        {
            return _mapper.Map<IEnumerable<LiveLocationDTO>>(
                await _repo.GetAllLiveLocationsAsync());
        }

        public async Task<BusLocationDTO?> AddLocationAsync(GpsReceiveDTO dto)
        {
            ValidateGps(dto);

            var activeTrip = await _tripRepo.GetActiveByDeviceIdAsync(dto.DeviceId.Trim());
            if (activeTrip == null)
                return null;

            var location = new BusLocation
            {
                BusId = activeTrip.BusId,
                TripId = activeTrip.Id,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Speed = dto.Speed,
                Heading = dto.Heading,
                Accuracy = dto.Accuracy,
                Status = dto.Status,
                Signal = dto.Signal,
                Timestamp = DateTime.UtcNow
            };

            var created = await _repo.CreateAsync(location);
            await CheckPickupNotificationsAsync(activeTrip, created);

            return _mapper.Map<BusLocationDTO>(created);
        }

        public async Task<IEnumerable<BusLocationDTO>> GetLocationsByDateRangeAsync(
            int busId,
            DateTime from,
            DateTime to)
        {
            return _mapper.Map<IEnumerable<BusLocationDTO>>(
                await _repo.GetByDateRangeAsync(busId, from, to));
        }

        public async Task<bool> DeleteOldLocationsAsync(int daysOld)
        {
            return await _repo.DeleteOldLocationsAsync(daysOld);
        }

        public async Task<EtaDTO?> GetEtaToPickupAsync(int reservationId)
        {
            var reservation = await _reservationRepo.GetByIdAsync(reservationId);
            if (reservation?.Trip == null)
                return null;

            var userIdValue = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdValue, out var userId))
                throw new UnauthorizedAccessException("User not authenticated.");

            if (role != UserRole.Admin.ToString() && reservation.UserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not allowed to view ETA for this reservation.");

            if (reservation.Trip.Status is not (TripStatus.InProgress or TripStatus.Delayed))
                return null;

            var latest = await _repo.GetLatestByTripIdAsync(reservation.Trip.Id);
            if (latest == null || latest.Timestamp < DateTime.UtcNow.AddMinutes(-5))
                return null;

            var distanceKm = GeoHelper.CalculateDistanceKm(
                latest.Latitude,
                latest.Longitude,
                reservation.PickupLatitude,
                reservation.PickupLongitude);
            var speedKmh = latest.Speed.HasValue && latest.Speed.Value >= 30
                ? Math.Min(latest.Speed.Value, 65)
                : 45;
            var minutes = GeoHelper.EstimateRoadMinutesAway(distanceKm, latest.Speed);

            return new EtaDTO
            {
                BusId = reservation.Trip.BusId,
                BusName = reservation.Trip.Bus.Name,
                ReservationId = reservation.Id,
                PickupPlaceName = reservation.PickupPlaceName,
                DistanceKm = Math.Round(distanceKm, 2),
                SpeedKmh = speedKmh,
                EstimatedMinutes = minutes,
                Message = $"Estimated arrival to {reservation.PickupPlaceName}: {minutes} {(minutes == 1 ? "minute" : "minutes")}."
            };
        }

        private async Task CheckPickupNotificationsAsync(Trip trip, BusLocation location)
        {
            var reservations = await _reservationRepo.GetByTripIdAsync(trip.Id);

            foreach (var reservation in reservations)
            {
                var distanceKm = GeoHelper.CalculateDistanceKm(
                    location.Latitude,
                    location.Longitude,
                    reservation.PickupLatitude,
                    reservation.PickupLongitude);
                var minutesAway = GeoHelper.EstimateRoadMinutesAway(distanceKm, location.Speed);

                if (minutesAway > 10 ||
                    await _notificationRepo.ExistsAsync(
                        trip.Id,
                        reservation.UserId,
                        reservation.Id,
                        NotificationType.PickupTenMinutesAway))
                    continue;

                await _notificationRepo.CreateAsync(new Notification
                {
                    TripId = trip.Id,
                    UserId = reservation.UserId,
                    ReservationId = reservation.Id,
                    Type = NotificationType.PickupTenMinutesAway,
                    Message = $"The bus is about {minutesAway} {(minutesAway == 1 ? "minute" : "minutes")} away from your pickup place: {reservation.PickupPlaceName}.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        private static void ValidateGps(GpsReceiveDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.DeviceId))
                throw new InvalidOperationException("Device ID is required.");
            if (dto.Latitude is < -90 or > 90)
                throw new InvalidOperationException("Invalid latitude.");
            if (dto.Longitude is < -180 or > 180)
                throw new InvalidOperationException("Invalid longitude.");
            if (dto.Latitude == 0 && dto.Longitude == 0)
                throw new InvalidOperationException("GPS fix not available.");
        }
    }
}
