using BusTrackingAPI.DTOs;
using BusTrackingAPI.Models;

namespace BusTrackingAPI.Services.Interfaces
{
    public interface IRecurringTripScheduleService
    {
        IReadOnlyList<TimetableOptionDTO> GetTimetable();

        Task<Trip> GetOrCreateTripAsync(
            string scheduleId,
            DateOnly travelDate,
            DateTime? utcNow = null);

        Task<int> RemoveUnusedGeneratedTripsAsync(
            DateTime? utcNow = null);
    }
}
