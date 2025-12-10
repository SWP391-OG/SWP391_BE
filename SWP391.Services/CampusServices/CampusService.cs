using AutoMapper;
using SWP391.Contracts.Campus;
using SWP391.Contracts.Location;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Services.CampusServices
{
    public class CampusService : ICampusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CampusService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<CampusDto>> GetAllCampus()
        {
           var campuses = await _unitOfWork.CampusRepository.GetAllAsync();
              return _mapper.Map<List<CampusDto>>(campuses);
        }

        public async Task<CampusDto> GetCampusByCode(string campusCode)
        {
            var campus = await _unitOfWork.CampusRepository.GetCampusByCodeAsync(campusCode);
            return _mapper.Map<CampusDto>(campus);
        }

        public async Task<List<LocationDto>> GetLocationsByCampusCodeAsync(string campusCode)
        {
            if (string.IsNullOrWhiteSpace(campusCode))
                return new List<LocationDto>();

            var locations = await _unitOfWork.CampusRepository.GetLocationByCampusCodeAsync(campusCode);
            return _mapper.Map<List<LocationDto>>(locations);
        }
    }
}
