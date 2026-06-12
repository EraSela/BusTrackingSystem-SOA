using AutoMapper;
using BusTrackingAPI.DTOs;
using BusTrackingAPI.Models;
using BusTrackingAPI.Repositories.Interfaces;
using BusTrackingAPI.Services.Interfaces;
using System.Security.Claims;

namespace BusTrackingAPI.Services.Implementations
{
    public class TripService : ITripService
    {
        private readonly ITripRepository _repo;
        private readonly IReservationRepository _reservationRepo;
        private readonly INotificationRepository _notificationRepo;
        private readonly IBusRepository _busRepo;
        private readonly IUserRepository _userRepo;
        private readonly IRouteRepository _routeRepo;
        private readonly IRecurringTripScheduleService _recurringSchedule;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TripService(
            ITripRepository repo,
            IReservationRepository reservationRepo,
            INotificationRepository notificationRepo,
            IBusRepository busRepo,
            IUserRepository userRepo,
            IRouteRepository routeRepo,
            IRecurringTripScheduleService recurringSchedule,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _reservationRepo = reservationRepo;
            _notificationRepo = notificationRepo;
            _busRepo = busRepo;
            _userRepo = userRepo;
            _routeRepo = routeRepo;
            _recurringSchedule = recurringSchedule;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<TripDTO>> GetAllAsync()
        {
            await _recurringSchedule.EnsureUpcomingTripsAsync();
            var data = await _repo.GetAllAsync();

            var user = _httpContextAccessor.HttpContext?.User;
            var role = user?.FindFirst(ClaimTypes.Role)?.Value;

            if (role == UserRole.Driver.ToString())
            {
                var userIdValue = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdValue, out var driverId))
                    throw new UnauthorizedAccessException("Driver account could not be identified.");

                data = data.Where(trip => trip.DriverId == driverId);
            }

            return _mapper.Map<IEnumerable<TripDTO>>(data);
        }

        public async Task<TripDTO?> GetByIdAsync(int id)
        {
            var data = await _repo.GetByIdAsync(id);

            if (data != null && !CanCurrentUserView(data))
                return null;

            return _mapper.Map<TripDTO?>(data);
        }

        public async Task<IEnumerable<TripDTO>> GetByBusIdAsync(int busId)
        {
            var data = await _repo.GetByBusIdAsync(busId);

            if (IsCurrentUserDriver(out var driverId))
                data = data.Where(trip => trip.DriverId == driverId);

            return _mapper.Map<IEnumerable<TripDTO>>(data);
        }

        public async Task<IEnumerable<TripDTO>> GetCompletedTripsAsync()
        {
            var data = await _repo.GetCompletedTripsAsync();
            return _mapper.Map<IEnumerable<TripDTO>>(data);
        }

        public async Task<TripDTO?> GetActiveByBusIdAsync(int busId)
        {
            var data = await _repo.GetActiveByBusIdAsync(busId);

            if (data != null && !CanCurrentUserView(data))
                return null;

            return _mapper.Map<TripDTO?>(data);
        }

        public async Task<TripDTO> CreateAsync(CreateTripDTO dto)
        {
            var bus = await _busRepo.GetByIdAsync(dto.BusId);
            if (bus == null)
                throw new Exception("Bus does not exist.");

            if (!bus.IsActive)
                throw new InvalidOperationException("Cannot schedule a trip for an inactive bus.");

            var driver = await _userRepo.GetByIdAsync(dto.DriverId);
            if (driver == null || driver.Role != UserRole.Driver || !driver.IsActive)
                throw new InvalidOperationException("Assigned driver must be an active driver account.");

            var route = await _routeRepo.GetByIdAsync(dto.RouteId);
            if (route == null || !route.IsActive)
                throw new InvalidOperationException("Selected route is not available.");

            if (string.IsNullOrWhiteSpace(dto.DeviceId))
                throw new InvalidOperationException("A GPS device ID is required.");

            var scheduledDeparture = dto.ScheduledDeparture.ToUniversalTime();
            var scheduledArrival = scheduledDeparture.AddMinutes(route.ExpectedDurationMinutes);

            if (await _repo.HasOverlappingBusTripAsync(
                    dto.BusId,
                    scheduledDeparture,
                    scheduledArrival))
                throw new InvalidOperationException("This bus already has a trip during that time.");

            if (await _repo.HasOverlappingDriverTripAsync(
                    dto.DriverId,
                    scheduledDeparture,
                    scheduledArrival))
                throw new InvalidOperationException("This driver is already assigned to another trip during that time.");

            if (await _repo.HasOverlappingDeviceTripAsync(
                    dto.DeviceId.Trim(),
                    scheduledDeparture,
                    scheduledArrival))
                throw new InvalidOperationException("This GPS device is already assigned to another trip during that time.");

            var trip = _mapper.Map<Trip>(dto);
            trip.DeviceId = dto.DeviceId.Trim();
            trip.ScheduledDeparture = scheduledDeparture;
            trip.ScheduledArrival = scheduledArrival;

            trip.Status = TripStatus.Scheduled;

            var created = await _repo.CreateAsync(trip);

            return _mapper.Map<TripDTO>(created);
        }

        public async Task<TripDTO?> UpdateStatusAsync(int id, UpdateTripStatusDTO dto)
        {
            if (!Enum.IsDefined(typeof(TripStatus), dto.Status))
                throw new InvalidOperationException("Invalid trip status.");

            var trip = await _repo.GetByIdAsync(id);

            if (trip == null)
                return null;

            EnsureDriverCanManage(trip);

            if (trip.Status is TripStatus.Completed or TripStatus.Cancelled)
                throw new InvalidOperationException($"{trip.Status} trips cannot be modified.");

            if (trip.Status == TripStatus.Scheduled)
            {
                if (dto.Status == TripStatus.Cancelled)
                {
                    EnsureNoTimes(dto);
                    trip.Status = TripStatus.Cancelled;
                }
                else if (dto.Status is TripStatus.InProgress or TripStatus.Delayed)
                {
                    if (!dto.ActualDeparture.HasValue)
                        throw new InvalidOperationException("Starting a trip requires an actual departure time.");

                    if (dto.ActualArrival.HasValue)
                        throw new InvalidOperationException("A trip cannot arrive when it is being started.");

                    await StartTripAsync(trip, dto.ActualDeparture.Value);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot change a scheduled trip to {dto.Status}.");
                }
            }
            else if (trip.Status is TripStatus.InProgress or TripStatus.Delayed)
            {
                if (dto.Status == TripStatus.Completed)
                {
                    if (!dto.ActualArrival.HasValue)
                        throw new InvalidOperationException("Completing a trip requires an actual arrival time.");

                    if (dto.ActualDeparture.HasValue)
                        throw new InvalidOperationException("Actual departure cannot be changed after the trip starts.");

                    var actualArrival = dto.ActualArrival.Value.ToUniversalTime();

                    if (!trip.ActualDeparture.HasValue || actualArrival <= trip.ActualDeparture.Value)
                        throw new InvalidOperationException("Arrival time must be after departure time.");

                    trip.ActualArrival = actualArrival;
                    trip.Status = TripStatus.Completed;
                }
                else if (dto.Status == TripStatus.Cancelled)
                {
                    EnsureNoTimes(dto);
                    trip.Status = TripStatus.Cancelled;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot change an active trip to {dto.Status}.");
                }
            }

            var updated = await _repo.UpdateAsync(trip);

            return _mapper.Map<TripDTO>(updated);
        }

        private async Task StartTripAsync(Trip trip, DateTime actualDeparture)
        {
            if (string.IsNullOrWhiteSpace(trip.DeviceId))
                throw new InvalidOperationException("Trip must have a tracker device before it can start.");

            var activeDeviceTrip = await _repo.GetActiveByDeviceIdAsync(trip.DeviceId);

            if (activeDeviceTrip != null && activeDeviceTrip.Id != trip.Id)
                throw new InvalidOperationException(
                    "This tracker device is already assigned to another active trip.");

            trip.ActualDeparture = actualDeparture.ToUniversalTime();

            var scheduledDepartureUtc = trip.ScheduledDeparture.ToUniversalTime();
            var delayMinutes =
                (int)(trip.ActualDeparture.Value - scheduledDepartureUtc).TotalMinutes;

            trip.Status = delayMinutes > 5
                ? TripStatus.Delayed
                : TripStatus.InProgress;

            if (trip.Status != TripStatus.Delayed)
                return;

            var reservations = await _reservationRepo.GetByTripIdAsync(trip.Id);

            foreach (var reservation in reservations)
            {
                var alreadySent = await _notificationRepo.ExistsAsync(
                    trip.Id,
                    reservation.UserId,
                    reservation.Id,
                    NotificationType.Delay);

                if (!alreadySent)
                {
                    await _notificationRepo.CreateAsync(new Notification
                    {
                        TripId = trip.Id,
                        UserId = reservation.UserId,
                        ReservationId = reservation.Id,
                        Type = NotificationType.Delay,
                        Message = $"The bus left with {delayMinutes} minutes delay.",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        private void EnsureDriverCanManage(Trip trip)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var role = user?.FindFirst(ClaimTypes.Role)?.Value;

            if (role == UserRole.Admin.ToString())
                return;

            var userIdValue = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out var userId) || trip.DriverId != userId)
                throw new UnauthorizedAccessException(
                    "You are not assigned to this trip.");
        }

        private bool CanCurrentUserView(Trip trip)
        {
            return !IsCurrentUserDriver(out var driverId) || trip.DriverId == driverId;
        }

        private bool IsCurrentUserDriver(out int driverId)
        {
            driverId = 0;
            var user = _httpContextAccessor.HttpContext?.User;
            var role = user?.FindFirst(ClaimTypes.Role)?.Value;

            if (role != UserRole.Driver.ToString())
                return false;

            var userIdValue = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out driverId))
                throw new UnauthorizedAccessException("Driver account could not be identified.");

            return true;
        }

        private static void EnsureNoTimes(UpdateTripStatusDTO dto)
        {
            if (dto.ActualDeparture.HasValue || dto.ActualArrival.HasValue)
                throw new InvalidOperationException(
                    "Cancellation must not include departure or arrival times.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}
