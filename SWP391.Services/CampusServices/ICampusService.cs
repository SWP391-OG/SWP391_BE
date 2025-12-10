using SWP391.Contracts.Campus;
using SWP391.Contracts.Location;
using SWP391.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Services.CampusServices
{
    public interface ICampusService
    {
        Task<List<CampusDto>> GetAllCampus();
        Task<CampusDto> GetCampusByCode(string campusCode);
        Task<List<LocationDto>> GetLocationsByCampusCodeAsync(string campusCode);
    }
}
