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
    /// <summary>
    /// Service for managing campus and location operations
    /// </summary>
    public class CampusService : ICampusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CampusService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all campuses
        /// </summary>
        public async Task<List<CampusDto>> GetAllCampus()
        {
           var campuses = await _unitOfWork.CampusRepository.GetAllAsync();
              return _mapper.Map<List<CampusDto>>(campuses);
        }

        /// <summary>
        /// Get campus by campus code
        /// </summary>
        public async Task<CampusDto> GetCampusByCode(string campusCode)
        {
             if (string.IsNullOrWhiteSpace(campusCode))
                return null;
            var campus = await _unitOfWork.CampusRepository.GetCampusByCodeAsync(campusCode);

            if(campus == null)
                return null;

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
