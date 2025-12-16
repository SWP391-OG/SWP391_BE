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

        // ✅ Get location with Campus data
        public async Task<Location?> GetLocationByCodeWithCampusAsync(string code)
        {
            var location = await _context.Locations
                .Include(l => l.Campus)
                .FirstOrDefaultAsync(d => d.LocationCode == code);
            return location;
        }

        // ✅ Get all locations with Campus data
        public async Task<List<Location>> GetAllLocationsWithCampusAsync()
        {
            var locations = await _context.Locations
                .Include(l => l.Campus)
                .ToListAsync();
            return locations;
        }

        public async Task<List<Location>> GetAllActiveLocationsAsync()
        {       
            var locations = await _context.Locations
                .Include(l => l.Campus)
                .Where(l => l.Status == "ACTIVE")
                .ToListAsync();
            return locations;
        }
    }
    }
