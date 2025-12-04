using SWP391.Repositories.Basic;
using SWP391.Repositories.DBContext;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Repositories.Repositories
{
    public class LocationRepository : GenericRepository<Location>, ILocationRepository
    {
        public LocationRepository() => _context ??= new FPTechnicalContext();

        public LocationRepository(FPTechnicalContext context) => _context = context;

        public async Task<Location?> GetLocationByCodeAsync(string code)
        {
            var location = await _context.Locations.FirstOrDefaultAsync(d => d.LocationCode == code);
            return location;
        }

        public async Task<Location?> GetLocationByNameAsync(string name)
        {
            var location = await _context.Locations.FirstOrDefaultAsync(d => d.LocationName == name);
            return location;
        }
    }
}
