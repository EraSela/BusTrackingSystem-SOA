using AutoMapper;
using BusTrackingAPI.DTOs;
using BusTrackingAPI.Models;

namespace BusTrackingAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // =========================
            // 👤 USER MAPPINGS
            // =========================
            CreateMap<User, UserDTO>();

            CreateMap<RegisterDTO, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // =========================
            // 🚌 BUS MAPPINGS
            // =========================
            CreateMap<Bus, BusDTO>();

            CreateMap<CreateBusDTO, Bus>();

            CreateMap<UpdateBusDTO, Bus>()
                .ForAllMembers(opt => opt.Condition(
                    (src, dest, srcMember) => srcMember != null));

            // =========================
            // 📍 BUS LOCATION (DB → DTO)
            // =========================
            CreateMap<BusLocation, BusLocationDTO>()
                .ForMember(dest => dest.BusName,
                    opt => opt.MapFrom(src => src.Bus.Name));

            CreateMap<BusLocation, LiveLocationDTO>()
                .ForMember(dest => dest.BusName,
                    opt => opt.MapFrom(src => src.Bus.Name))
                .ForMember(dest => dest.ScheduledDeparture,
                    opt => opt.MapFrom(src => src.Trip != null
                        ? src.Trip.ScheduledDeparture
                        : (DateTime?)null))
                .ForMember(dest => dest.RouteName,
                    opt => opt.MapFrom(src => src.Trip != null && src.Trip.Route != null
                        ? src.Trip.Route.Name
                        : string.Empty))
                .ForMember(dest => dest.LastUpdated,
                    opt => opt.MapFrom(src => src.Timestamp));

            // =========================
            // 📡 ARDUINO GPS INPUT (IMPORTANT)
            // =========================
            CreateMap<GpsReceiveDTO, BusLocation>()
                .ForMember(dest => dest.BusId, opt => opt.Ignore())
                .ForMember(dest => dest.Timestamp, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<CreateBusLocationDTO, BusLocation>();

            // =========================
            // 🎫 RESERVATION MAPPINGS
            // =========================
            CreateMap<Reservation, ReservationDTO>()
                .ForMember(dest => dest.UserFullName,
                    opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.BusName,
                    opt => opt.MapFrom(src => src.Trip != null ? src.Trip.Bus.Name : string.Empty))
                .ForMember(dest => dest.BusId,
                    opt => opt.MapFrom(src => src.Trip != null ? src.Trip.BusId : 0))
                .ForMember(dest => dest.RouteName,
                    opt => opt.MapFrom(src => src.Trip != null && src.Trip.Route != null ? src.Trip.Route.Name : string.Empty))
                .ForMember(dest => dest.ScheduledDeparture,
                    opt => opt.MapFrom(src => src.Trip != null ? src.Trip.ScheduledDeparture : default))
                .ForMember(dest => dest.ScheduledArrival,
                    opt => opt.MapFrom(src => src.Trip != null ? src.Trip.ScheduledArrival : default));

            CreateMap<CreateReservationDTO, Reservation>()
                .ForMember(dest => dest.IsVerified,
                    opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => DateTime.UtcNow));
           
            // 🛣️ TRIP MAPPINGS
            // =========================
            CreateMap<Trip, TripDTO>()
                .ForMember(dest => dest.BusName,
                    opt => opt.MapFrom(src => src.Bus.Name))
                .ForMember(dest => dest.TotalSeats,
                    opt => opt.MapFrom(src => src.Bus.TotalSeats))
                .ForMember(dest => dest.ReservationCount,
                    opt => opt.MapFrom(src => src.Reservations.Count))
                .ForMember(dest => dest.DriverName,
                    opt => opt.MapFrom(src => src.Driver != null ? src.Driver.FullName : string.Empty))
                .ForMember(dest => dest.RouteName,
                    opt => opt.MapFrom(src => src.Route != null ? src.Route.Name : string.Empty))
                .ForMember(dest => dest.Origin,
                    opt => opt.MapFrom(src => src.Route != null ? src.Route.Origin : string.Empty))
                .ForMember(dest => dest.Destination,
                    opt => opt.MapFrom(src => src.Route != null ? src.Route.Destination : string.Empty))
                .ForMember(dest => dest.ExpectedDurationMinutes,
                    opt => opt.MapFrom(src => src.Route != null ? src.Route.ExpectedDurationMinutes : 0))

                .ForMember(dest => dest.DelayMinutes,
                    opt => opt.MapFrom(src => src.DelayMinutes))

                .ForMember(dest => dest.DelayMessage,
                    opt => opt.MapFrom(src => src.DelayMessage));

            CreateMap<CreateTripDTO, Trip>();
            CreateMap<BusRoute, RouteDTO>();
            CreateMap<CreateRouteDTO, BusRoute>();

            CreateMap<Notification, NotificationDTO>();


        }
    }
}
