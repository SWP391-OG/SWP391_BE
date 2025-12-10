using SWP391.Contracts.Location;
using SWP391.Contracts.Role;
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
        Task<List<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> GetRoleByNameAsync(string roleName);
        Task<(bool Success, string Message, RoleDto Data)> CreateRoleAsync(string roleName);
        Task<(bool Success, string Message)> UpdateRoleAsync(RoleDto role);
        Task<(bool Success, string Message)> DeleteRoleAsync(int id);
    }
}
