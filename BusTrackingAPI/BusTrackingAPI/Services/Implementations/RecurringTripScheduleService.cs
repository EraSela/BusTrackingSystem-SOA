using BusTrackingAPI.Data;
using BusTrackingAPI.DTOs;
using BusTrackingAPI.Models;
using BusTrackingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTrackingAPI.Services.Implementations
{
    public class RecurringTripScheduleService : IRecurringTripScheduleService
    {
        private static readonly SemaphoreSlim CreationLock = new(1, 1);
        private static readonly DayOfWeek[] LimitedServiceDays =
        {
            DayOfWeek.Monday,
            DayOfWeek.Friday,
            DayOfWeek.Sunday
        };

        private static readonly ScheduleDefinition[] Schedule =
        {
            new("struga-skopje-0500", "Struga", "Skopje", "Struga - Tetovo - Skopje", new TimeSpan(5, 0, 0), 180, "BUS-0500", "Bus 1", "SIM808_01", false),
            new("struga-skopje-0800", "Struga", "Skopje", "Struga - Tetovo - Skopje", new TimeSpan(8, 0, 0), 180, "BUS-0800", "Bus 2", "SIM808_02", false),
            new("struga-skopje-1200", "Struga", "Skopje", "Struga - Tetovo - Skopje", new TimeSpan(12, 0, 0), 180, "BUS-1200", "Bus 3", "SIM808_03", false),
            new("struga-skopje-1700", "Struga", "Skopje", "Struga - Tetovo - Skopje", new TimeSpan(17, 0, 0), 180, "BUS-1700", "Bus 10", "SIM808_10", true),

            new("skopje-struga-1100", "Skopje", "Struga", "Skopje - Tetovo - Struga", new TimeSpan(11, 0, 0), 180, "BUS-1100", "Bus 4", "SIM808_04", false),
            new("skopje-struga-1315", "Skopje", "Struga", "Skopje - Tetovo - Struga", new TimeSpan(13, 15, 0), 180, "BUS-1315", "Bus 5", "SIM808_05", false),
            new("skopje-struga-1620", "Skopje", "Struga", "Skopje - Tetovo - Struga", new TimeSpan(16, 20, 0), 180, "BUS-1620", "Bus 6", "SIM808_06", false),
            new("skopje-struga-2100", "Skopje", "Struga", "Skopje - Tetovo - Struga", new TimeSpan(21, 0, 0), 180, "BUS-2100", "Bus 11", "SIM808_11", true),

            new("tetovo-struga-1200", "Tetovo", "Struga", "Tetovo - Struga", new TimeSpan(12, 0, 0), 120, "BUS-T1200", "Bus 7", "SIM808_07", false),
            new("tetovo-struga-1415", "Tetovo", "Struga", "Tetovo - Struga", new TimeSpan(14, 15, 0), 120, "BUS-T1415", "Bus 8", "SIM808_08", false),
            new("tetovo-struga-1720", "Tetovo", "Struga", "Tetovo - Struga", new TimeSpan(17, 20, 0), 120, "BUS-T1720", "Bus 9", "SIM808_09", false)
        };

        private readonly AppDbContext _context;

        public RecurringTripScheduleService(AppDbContext context)
        {
            _context = context;
        }

        public IReadOnlyList<TimetableOptionDTO> GetTimetable()
        {
            return Schedule.Select(definition => new TimetableOptionDTO
            {
                Id = definition.Id,
                RouteName = definition.RouteName,
                Origin = definition.Origin,
                Destination = definition.Destination,
                DepartureTime = definition.DepartureTime.ToString(@"hh\:mm"),
                ExpectedDurationMinutes = definition.DurationMinutes,
                TotalSeats = 40,
                AvailableDays = definition.LimitedDaysOnly
                    ? LimitedServiceDays.Select(day => (int)day).ToArray()
                    : Enum.GetValues<DayOfWeek>().Select(day => (int)day).ToArray()
            }).ToArray();
        }

        public async Task<Trip> GetOrCreateTripAsync(
            string scheduleId,
            DateOnly travelDate,
            DateTime? utcNow = null)
        {
            var definition = Schedule.FirstOrDefault(item =>
                string.Equals(item.Id, scheduleId, StringComparison.OrdinalIgnoreCase));
            if (definition == null)
                throw new InvalidOperationException("Selected timetable departure does not exist.");

            var localDate = travelDate.ToDateTime(TimeOnly.MinValue);
            if (definition.LimitedDaysOnly &&
                !LimitedServiceDays.Contains(localDate.DayOfWeek))
                throw new InvalidOperationException(
                    "This departure is available only on Monday, Friday, and Sunday.");

            var timeZone = GetSkopjeTimeZone();
            var localDeparture = DateTime.SpecifyKind(
                localDate.Add(definition.DepartureTime),
                DateTimeKind.Unspecified);
            var departureUtc = TimeZoneInfo.ConvertTimeToUtc(localDeparture, timeZone);
            var now = (utcNow ?? DateTime.UtcNow).ToUniversalTime();

            if (departureUtc <= now)
                throw new InvalidOperationException(
                    "Please select a future departure date and time.");

            await CreationLock.WaitAsync();
            try
            {
                var bus = await EnsureBusAsync(definition);
                var route = await EnsureRouteAsync(definition);

                var existing = await _context.Trips
                    .Include(trip => trip.Bus)
                    .Include(trip => trip.Route)
                    .FirstOrDefaultAsync(trip =>
                        trip.RouteId == route.Id &&
                        trip.ScheduledDeparture == departureUtc &&
                        trip.Status != TripStatus.Cancelled);
                if (existing != null)
                    return existing;

                var trip = new Trip
                {
                    BusId = bus.Id,
                    Bus = bus,
                    RouteId = route.Id,
                    Route = route,
                    DeviceId = definition.DeviceId,
                    ScheduledDeparture = departureUtc,
                    ScheduledArrival = departureUtc.AddMinutes(
                        definition.DurationMinutes),
                    Status = TripStatus.Scheduled,
                    CreatedAt = now
                };

                _context.Trips.Add(trip);
                await _context.SaveChangesAsync();
                return trip;
            }
            finally
            {
                CreationLock.Release();
            }
        }

        public async Task<int> RemoveUnusedGeneratedTripsAsync(
            DateTime? utcNow = null)
        {
            var now = (utcNow ?? DateTime.UtcNow).ToUniversalTime();
            var deviceIds = Schedule.Select(item => item.DeviceId).ToArray();
            var unusedTrips = await _context.Trips
                .Where(trip =>
                    trip.DriverId == null &&
                    trip.Status == TripStatus.Scheduled &&
                    trip.ScheduledDeparture > now &&
                    trip.DeviceId != null &&
                    deviceIds.Contains(trip.DeviceId) &&
                    !trip.Reservations.Any())
                .ToListAsync();

            if (unusedTrips.Count == 0)
                return 0;

            _context.Trips.RemoveRange(unusedTrips);
            await _context.SaveChangesAsync();
            return unusedTrips.Count;
        }

        private async Task<Bus> EnsureBusAsync(ScheduleDefinition definition)
        {
            var bus = await _context.Buses.FirstOrDefaultAsync(item =>
                item.PlateNumber == definition.PlateNumber);
            if (bus != null)
                return bus;

            bus = new Bus
            {
                Name = definition.BusName,
                PlateNumber = definition.PlateNumber,
                TotalSeats = 40,
                IsActive = true
            };
            _context.Buses.Add(bus);
            await _context.SaveChangesAsync();
            return bus;
        }

        private async Task<BusRoute> EnsureRouteAsync(
            ScheduleDefinition definition)
        {
            var route = await _context.Routes.FirstOrDefaultAsync(item =>
                item.Origin == definition.Origin &&
                item.Destination == definition.Destination);
            if (route != null)
                return route;

            route = new BusRoute
            {
                Name = definition.RouteName,
                Origin = definition.Origin,
                Destination = definition.Destination,
                ExpectedDurationMinutes = definition.DurationMinutes,
                IsActive = true
            };
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
            return route;
        }

        private static TimeZoneInfo GetSkopjeTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Skopje");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById(
                    "Central European Standard Time");
            }
        }

        private sealed record ScheduleDefinition(
            string Id,
            string Origin,
            string Destination,
            string RouteName,
            TimeSpan DepartureTime,
            int DurationMinutes,
            string PlateNumber,
            string BusName,
            string DeviceId,
            bool LimitedDaysOnly);
    }
}
