using BusTrackingAPI.Data;
using BusTrackingAPI.Models;
using BusTrackingAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusTrackingAPI.Services.Implementations
{
    public class RecurringTripScheduleService : IRecurringTripScheduleService
    {
        private static readonly SemaphoreSlim GenerationLock = new(1, 1);
        private static readonly DayOfWeek[] LimitedServiceDays =
        {
            DayOfWeek.Monday,
            DayOfWeek.Friday,
            DayOfWeek.Sunday
        };

        private static readonly ScheduleDefinition[] Schedule =
        {
            new("Struga", "Skopje", new TimeSpan(5, 0, 0), "BUS-0500", "Bus 1", "SIM808_01", false),
            new("Struga", "Skopje", new TimeSpan(8, 0, 0), "BUS-0800", "Bus 2", "SIM808_02", false),
            new("Struga", "Skopje", new TimeSpan(12, 0, 0), "BUS-1200", "Bus 3", "SIM808_03", false),
            new("Struga", "Skopje", new TimeSpan(17, 0, 0), "BUS-1700", "Bus 10", "SIM808_10", true),

            new("Skopje", "Struga", new TimeSpan(11, 0, 0), "BUS-1100", "Bus 4", "SIM808_04", false),
            new("Skopje", "Struga", new TimeSpan(13, 15, 0), "BUS-1315", "Bus 5", "SIM808_05", false),
            new("Skopje", "Struga", new TimeSpan(16, 20, 0), "BUS-1620", "Bus 6", "SIM808_06", false),
            new("Skopje", "Struga", new TimeSpan(21, 0, 0), "BUS-2100", "Bus 11", "SIM808_11", true),

            new("Tetovo", "Struga", new TimeSpan(12, 0, 0), "BUS-T1200", "Bus 7", "SIM808_07", false),
            new("Tetovo", "Struga", new TimeSpan(14, 15, 0), "BUS-T1415", "Bus 8", "SIM808_08", false),
            new("Tetovo", "Struga", new TimeSpan(17, 20, 0), "BUS-T1720", "Bus 9", "SIM808_09", false)
        };

        private readonly AppDbContext _context;

        public RecurringTripScheduleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> EnsureUpcomingTripsAsync(
            DateTime? utcNow = null,
            int daysAhead = 30)
        {
            if (daysAhead < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(daysAhead),
                    "Days ahead cannot be negative.");

            await GenerationLock.WaitAsync();
            try
            {
                var now = (utcNow ?? DateTime.UtcNow).ToUniversalTime();
                var timeZone = GetSkopjeTimeZone();
                var localStartDate = TimeZoneInfo.ConvertTimeFromUtc(now, timeZone).Date;
                var localEndDate = localStartDate.AddDays(daysAhead);

                var buses = await EnsureBusesAsync();
                var routes = await EnsureRoutesAsync();

                var rangeStartUtc = TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.SpecifyKind(localStartDate, DateTimeKind.Unspecified),
                    timeZone);
                var rangeEndUtc = TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.SpecifyKind(localEndDate.AddDays(1), DateTimeKind.Unspecified),
                    timeZone);

                var existingKeys = (await _context.Trips
                        .Where(trip =>
                            trip.RouteId != null &&
                            trip.ScheduledDeparture >= rangeStartUtc &&
                            trip.ScheduledDeparture < rangeEndUtc)
                        .Select(trip => new
                        {
                            RouteId = trip.RouteId!.Value,
                            trip.ScheduledDeparture
                        })
                        .ToListAsync())
                    .Select(item => CreateTripKey(
                        item.RouteId,
                        item.ScheduledDeparture))
                    .ToHashSet();

                var created = 0;

                for (var date = localStartDate; date <= localEndDate; date = date.AddDays(1))
                {
                    foreach (var definition in Schedule)
                    {
                        if (definition.LimitedDaysOnly &&
                            !LimitedServiceDays.Contains(date.DayOfWeek))
                            continue;

                        var localDeparture = DateTime.SpecifyKind(
                            date.Add(definition.DepartureTime),
                            DateTimeKind.Unspecified);
                        var departureUtc = TimeZoneInfo.ConvertTimeToUtc(
                            localDeparture,
                            timeZone);

                        if (departureUtc <= now)
                            continue;

                        var route = routes[(definition.Origin, definition.Destination)];
                        var key = CreateTripKey(route.Id, departureUtc);

                        if (!existingKeys.Add(key))
                            continue;

                        var bus = buses[definition.PlateNumber];
                        _context.Trips.Add(new Trip
                        {
                            BusId = bus.Id,
                            RouteId = route.Id,
                            DeviceId = definition.DeviceId,
                            ScheduledDeparture = departureUtc,
                            ScheduledArrival = departureUtc.AddMinutes(
                                route.ExpectedDurationMinutes),
                            Status = TripStatus.Scheduled,
                            CreatedAt = now
                        });
                        created++;
                    }
                }

                if (created > 0)
                    await _context.SaveChangesAsync();

                return created;
            }
            finally
            {
                GenerationLock.Release();
            }
        }

        private async Task<Dictionary<string, Bus>> EnsureBusesAsync()
        {
            var requiredPlates = Schedule
                .Select(item => item.PlateNumber)
                .Distinct()
                .ToArray();
            var buses = await _context.Buses
                .Where(bus => requiredPlates.Contains(bus.PlateNumber))
                .ToDictionaryAsync(bus => bus.PlateNumber);

            foreach (var definition in Schedule.DistinctBy(item => item.PlateNumber))
            {
                if (buses.ContainsKey(definition.PlateNumber))
                    continue;

                var bus = new Bus
                {
                    Name = definition.BusName,
                    PlateNumber = definition.PlateNumber,
                    TotalSeats = 40,
                    IsActive = true
                };
                _context.Buses.Add(bus);
                buses[definition.PlateNumber] = bus;
            }

            if (_context.ChangeTracker.HasChanges())
                await _context.SaveChangesAsync();

            return buses;
        }

        private async Task<Dictionary<(string Origin, string Destination), BusRoute>>
            EnsureRoutesAsync()
        {
            var routes = await _context.Routes
                .Where(route => route.IsActive)
                .ToListAsync();
            var result = routes.ToDictionary(
                route => (route.Origin, route.Destination));

            AddRouteIfMissing(
                result,
                "Struga",
                "Skopje",
                "Struga - Tetovo - Skopje",
                180);
            AddRouteIfMissing(
                result,
                "Skopje",
                "Struga",
                "Skopje - Tetovo - Struga",
                180);
            AddRouteIfMissing(
                result,
                "Tetovo",
                "Struga",
                "Tetovo - Struga",
                120);

            if (_context.ChangeTracker.HasChanges())
                await _context.SaveChangesAsync();

            return result;
        }

        private void AddRouteIfMissing(
            IDictionary<(string Origin, string Destination), BusRoute> routes,
            string origin,
            string destination,
            string name,
            int durationMinutes)
        {
            var key = (origin, destination);
            if (routes.ContainsKey(key))
                return;

            var route = new BusRoute
            {
                Name = name,
                Origin = origin,
                Destination = destination,
                ExpectedDurationMinutes = durationMinutes,
                IsActive = true
            };
            _context.Routes.Add(route);
            routes[key] = route;
        }

        private static string CreateTripKey(
            int routeId,
            DateTime departureUtc)
        {
            return $"{routeId}:{departureUtc.ToUniversalTime().Ticks}";
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
            string Origin,
            string Destination,
            TimeSpan DepartureTime,
            string PlateNumber,
            string BusName,
            string DeviceId,
            bool LimitedDaysOnly);
    }
}
