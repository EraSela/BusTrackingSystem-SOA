using BusTrackingAPI.Services.Interfaces;

namespace BusTrackingAPI.Services.Implementations
{
    public class RecurringTripScheduleWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RecurringTripScheduleWorker> _logger;

        public RecurringTripScheduleWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<RecurringTripScheduleWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scheduleService = scope.ServiceProvider
                        .GetRequiredService<IRecurringTripScheduleService>();
                    await scheduleService.EnsureUpcomingTripsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to generate recurring trips.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
