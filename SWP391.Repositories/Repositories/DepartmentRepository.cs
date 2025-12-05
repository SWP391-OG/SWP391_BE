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
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository() => _context ??= new FPTechnicalContext();

        public DepartmentRepository(FPTechnicalContext context) => _context = context;

        public async Task<Department?> GetDepartmentByCodeAsync(string code)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.DeptCode == code);
            return department;
        }

        public async Task<Department?> GetDepartmentByNameAsync(string name)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.DeptName == name);
            return department;
        }
    }
}
