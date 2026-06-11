using AutoMapper;
using BusTrackingAPI.DTOs;
using BusTrackingAPI.Repositories.Interfaces;
using BusTrackingAPI.Services.Interfaces;

namespace BusTrackingAPI.Services.Implementations
{
    public class RouteService : IRouteService
    {
        private readonly IRouteRepository _repository;
        private readonly IMapper _mapper;

        public RouteService(IRouteRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RouteDTO>> GetAllAsync()
        {
            return _mapper.Map<IEnumerable<RouteDTO>>(await _repository.GetAllAsync());
        }
    }
}
