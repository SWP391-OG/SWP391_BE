using AutoMapper;
using SWP391.Contracts.Location;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Services.LocationServices
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LocationService(IUnitOfWork unitOfWork, IMapper mapper) 
        { 
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<(bool Success, string Message, LocationDto Data)> CreateLocationAsync(LocationRequestDto dto)
        {
            var existingCode = await _unitOfWork.LocationRepository.GetLocationByCodeAsync(dto.LocationCode);
            if (existingCode != null)
                return (false, "Location code already exists", null);

            var existingName = await _unitOfWork.LocationRepository.GetLocationByNameAsync(dto.LocationName);
            if (existingName != null)
                return (false, "Location name already exists", null);

            var location = _mapper.Map<Location>(dto);
            await _unitOfWork.LocationRepository.CreateAsync(location);
            var locationDto = _mapper.Map<LocationDto>(location);
            return (true, "Location created successfully", locationDto);
        }

        public async Task<(bool Success, string Message)> DeleteLocationByCodeAsync(string locationCode)
        {
            var existingCode = await _unitOfWork.LocationRepository.GetLocationByCodeAsync(locationCode);
            if (existingCode == null)
                return (false, "Location code doesn't exists");
            await _unitOfWork.LocationRepository.RemoveAsync(existingCode);
            return (true, "Location deleted successfully");
        }


        
        public async Task<List<LocationDto>> GetAllLocationsAsync()
        {
          var locations = await _unitOfWork.LocationRepository.GetAllAsync();
            var exis = new List<LocationDto>();
            LocationDto existLocation;
            foreach (var location in locations)
            {
               existLocation =  _mapper.Map<LocationDto>(location);
                exis.Add(existLocation);
            }

            return exis;
        }

        public async Task<LocationDto> GetByLocationCodeAsync(string locationCode)
            => await _unitOfWork.LocationRepository.GetLocationByCodeAsync(locationCode)
                .ContinueWith(task => _mapper.Map<LocationDto>(task.Result));

        public async Task<(bool Success, string Message)> UpdateLocationAsync(LocationRequestDto dto)
        {
           var location = await _unitOfWork.LocationRepository.GetLocationByCodeAsync(dto.LocationCode);
            if(location == null)
            {
                return (false, "Location not found");
            }
            location.LocationCode = dto.LocationCode;
            location.LocationName = dto.LocationName;
            _unitOfWork.LocationRepository.Update(location);
            
            return (true, "Location updated successfully"); 
        }

       

        public async Task<(bool Success, string Message)> UpdateStatusLocationAsync(LocationStatusUpdateDto dto)
        {
          var location = await _unitOfWork.LocationRepository.GetLocationByCodeAsync(dto.LocationCode);


            if (location == null)
            {
                return (false, "Location not found");
            }
            location.Status = dto.Status;
            _unitOfWork.LocationRepository.Update(location);

            return (true, "Location status updated successfully");
        }

        
    }
}
