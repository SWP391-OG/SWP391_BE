using SWP391.Repositories.Models;
using SWP391.Contracts.Location;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Services.LocationServices
{
    public interface ILocationService
    {
        Task<List<LocationDto>> GetAllLocationsAsync();
        Task<LocationDto> GetByLocationCodeAsync(string locationCode);
        Task<(bool Success, string Message, LocationDto Data)> CreateLocationAsync(LocationRequestDto dto);
        Task<(bool Success, string Message)> UpdateLocationAsync(LocationRequestDto dto);
        Task<(bool Success, string Message)> UpdateStatusLocationAsync(LocationStatusUpdateDto dto);
        Task<(bool Success, string Message)> DeleteLocationByCodeAsync(string locationCode);


    }
}
