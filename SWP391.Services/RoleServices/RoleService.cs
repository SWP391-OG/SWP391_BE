using AutoMapper;
using SWP391.Contracts.Location;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Services.RoleServices
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<(bool Success, string Message, Role Data)> CreateRoleAsync(Role role)
        {
            var existingRole = await _unitOfWork.RoleRepository.GetByIdAsync(role.Id);
            if (existingRole != null)
                return (false, "Role already exists", null);

            var roleName = await _unitOfWork.RoleRepository.GetRoleByNameAsync(role.RoleName);
            if (roleName != null)
                return (false, "Role name already exists", null);
  
            await _unitOfWork.RoleRepository.CreateAsync(role);
           
            return (true, "Role created successfully", role);
        }

        public async Task<(bool Success, string Message)> DeleteRoleAsync(int roleId)
        {
            var existingRole = await _unitOfWork.RoleRepository.GetByIdAsync(roleId);
            if (existingRole == null)
                return (false, "Role code doesn't exists");

            await _unitOfWork.RoleRepository.RemoveAsync(existingRole);
            return (true, "Role deleted successfully");
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            var roles = await _unitOfWork.RoleRepository.GetAllAsync();
            return roles;
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
            => await _unitOfWork.RoleRepository.GetRoleByNameAsync(roleName);
         

        public async Task<(bool Success, string Message)> UpdateRoleAsync(Role role)
        {
            var existRole = await _unitOfWork.RoleRepository.GetRoleByNameAsync(role.RoleName);
            if (existRole == null)
            {
                return (false, "Role not found");
            }
            existRole.RoleName = role.RoleName;
            _unitOfWork.RoleRepository.Update(existRole);

            return (true, "Role updated successfully");
        }

    }
}
