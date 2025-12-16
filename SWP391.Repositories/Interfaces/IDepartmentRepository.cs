using SWP391.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Repositories.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<Department?> GetDepartmentByCodeAsync(string code);
        Task<Department?> GetDepartmentByNameAsync(string name);
        Task<List<Department>> GetAllActiveDepartmentAsync();
    }
}
