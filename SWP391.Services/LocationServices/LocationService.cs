using AutoMapper;
using SWP391.Contracts.Location;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;

namespace SWP391.Services.LocationServices
{
    /// <summary>
    /// Service for managing location operations
    /// </summary>
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LocationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        /// <summary>
        /// Create a new location
        /// </summary>
        public async Task<(bool Success, string Message, LocationDto Data)> CreateLocationAsync(LocationRequestDto dto)
        {
            // Validate required fields
            if (dto == null)
                return (false, "Location data cannot be null", null);

            if (string.IsNullOrWhiteSpace(dto.LocationCode))
                return (false, "Location code is required", null);

            if (string.IsNullOrWhiteSpace(dto.LocationName))
                return (false, "Location name is required", null);

            if (dto.CampusId <= 0)
                return (false, "Valid campus ID is required", null);

            // Check if location code already exists
            var existingCode = await _unitOfWork.LocationRepository.GetLocationByCodeAsync(dto.LocationCode);
            if (existingCode != null)
                return (false, "Location code already exists", null);

            // Check if location name already exists
            var existingName = await _unitOfWork.LocationRepository.GetLocationByNameAsync(dto.LocationName);
            if (existingName != null)
                return (false, "Location name already exists", null);

            // Validate that campus exists
            var campus = await _unitOfWork.CampusRepository.GetByIdAsync(dto.CampusId);
            if (campus == null)
                return (false, "Campus does not exist. Please provide a valid campus ID", null);

            var location = new Location
            {
                LocationCode = dto.LocationCode,
                LocationName = dto.LocationName,
                CampusId = dto.CampusId,
                Status = "ACTIVE",
                CreatedAt = DateTime.UtcNow,
                Campus = campus  // ✅ Set Campus navigation property
            };

            await _unitOfWork.LocationRepository.CreateAsync(location);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // ✅ Reload location with Campus data to ensure mapping works correctly
            var createdLocation = await _unitOfWork.LocationRepository.GetLocationByCodeWithCampusAsync(dto.LocationCode);
            var locationDto = _mapper.Map<LocationDto>(createdLocation);
            return (true, "Location created successfully", locationDto);
        }

        /// <summary>
        /// Delete location (soft delete by marking as inactive)
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteLocationByIdAsync(int locationId)
        {
             if (locationId <= 0)
                return (false, "Invalid location ID");

            var existingCode = await _unitOfWork.LocationRepository.GetByIdAsync(locationId);
            if (existingCode == null)
                return (false, "Location code doesn't exists");
                
            existingCode.Status = "INACTIVE"; // Soft delete by setting status to Inactive
             _unitOfWork.LocationRepository.Update(existingCode);
            return (true, "Location deleted successfully");
        }

        /// <summary>
        /// Get all active locations
        /// </summary>
        public async Task<List<LocationDto>> GetAllActiveLocationsAsync()
        {
            var locations = await _unitOfWork.LocationRepository.GetAllActiveLocationsAsync();
            return _mapper.Map<List<LocationDto>>(locations);
        }

        /// <summary>
        /// Get all locations (active and inactive)
        /// </summary>
        public async Task<List<LocationDto>> GetAllLocationsAsync()
        {
            var locations = await _unitOfWork.LocationRepository.GetAllLocationsWithCampusAsync();
            return _mapper.Map<List<LocationDto>>(locations);
        }

        /// <summary>
        /// Get location by location code
        /// </summary>
        public async Task<LocationDto> GetByLocationCodeAsync(string locationCode)
        {
              if (string.IsNullOrWhiteSpace(locationCode))
                return null;

            var location = await _unitOfWork.LocationRepository.GetLocationByCodeWithCampusAsync(locationCode);
              return location == null ? null : _mapper.Map<LocationDto>(location);
        }

        /// <summary>
        /// Get locations by campus code
        /// </summary>
        public async Task<List<LocationDto>> GetLocationsByCampusCodeAsync(string campusCode)
        {
            if (string.IsNullOrWhiteSpace(campusCode))
                return new List<LocationDto>();

           var locations =  await _unitOfWork.LocationRepository.GetLocationsByCampusCodeAsync(campusCode);
            return locations == null ? null : _mapper.Map<List<LocationDto>>(locations);
        }

        /// <summary>
        /// Update location information
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateLocationAsync(int locationId, LocationRequestDto dto)
        {
            if (locationId <= 0)
                return (false, "Invalid location ID");

            if (dto == null)
                return (false, "Location data cannot be null");

            var location = await _unitOfWork.LocationRepository.GetByIdAsync(locationId);
            if (location == null)
            {
                return (false, "Location not found");
            }

            // Validate campus exists if CampusId is provided
            if (dto.CampusId > 0 && dto.CampusId != location.CampusId)
            {
                var campus = await _unitOfWork.CampusRepository.GetByIdAsync(dto.CampusId);
                if (campus == null)
                    return (false, "Campus does not exist. Please provide a valid campus ID");

                location.CampusId = dto.CampusId;
            }

             // Update fields
            if (!string.IsNullOrWhiteSpace(dto.LocationName))
                location.LocationName = dto.LocationName;

            if (!string.IsNullOrWhiteSpace(dto.LocationCode))
                location.LocationCode = dto.LocationCode;

            await _unitOfWork.LocationRepository.UpdateAsync(location);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "Location updated successfully");
        }
        
        /// <summary>
        /// Update location status
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateStatusLocationAsync(LocationStatusUpdateDto dto)
        {
            if (dto == null)
                return (false, "Status update request cannot be null");

            if (dto.LocationId <= 0)
                return (false, "Invalid location ID");

            if (string.IsNullOrWhiteSpace(dto.Status))
                return (false, "Status cannot be empty");

            var location = await _unitOfWork.LocationRepository.GetByIdAsync(dto.LocationId);

            if (location == null)
            {
                return (false, "Location not found");
            }

            location.Status = dto.Status;
            await _unitOfWork.LocationRepository.UpdateAsync(location);
            return (true, "Location status updated successfully");
        }

        
    }
}
