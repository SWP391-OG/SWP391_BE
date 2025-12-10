using Microsoft.EntityFrameworkCore;
using SWP391.Repositories.Basic;
using SWP391.Repositories.DBContext;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Repositories.Repositories
{
    public class CampusRepository : GenericRepository<Campus>, ICampusRepository
    {
        public CampusRepository() => _context ??= new FPTechnicalContext();
        public CampusRepository(FPTechnicalContext context) => _context = context;

        public async Task<Campus?> GetCampusByCodeAsync(string code)
        {
            var campus = await _context.Campuses
                .FirstOrDefaultAsync(c => c.CampusCode == code);
            return campus;
        }

        public async Task<List<Location>> GetLocationByCampusCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return new List<Location>();

            var campus = await _context.Campuses
                .Include(c => c.Locations)
                .FirstOrDefaultAsync(c => c.CampusCode == code);
            
            return campus?.Locations.ToList() ?? new List<Location>();
        }
    }
}
