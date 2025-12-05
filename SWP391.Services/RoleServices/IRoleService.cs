using SWP391.Contracts.Location;
using SWP391.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Services.RoleServices
{
    public interface IRoleService
    {
        Task<List<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByNameAsync(string roleName);
        Task<(bool Success, string Message, Role Data)> CreateRoleAsync(Role role);
        Task<(bool Success, string Message)> UpdateRoleAsync(Role role);
        Task<(bool Success, string Message)> DeleteRoleAsync(int id);
    }
}
