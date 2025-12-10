using AutoMapper;
using SWP391.Contracts.Location;
using SWP391.Contracts.Role;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Data;
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

        public async Task<(bool Success, string Message, RoleDto Data)> CreateRoleAsync(string roleName)
        {
           

            var existingRole = await _unitOfWork.RoleRepository.GetRoleByNameAsync(roleName);
            if (existingRole != null)
                return (false, "Role name already exists", null);

            var newRole = new Role
            {
                RoleName = roleName,
            };

            await _unitOfWork.RoleRepository.CreateAsync(newRole);
            var roleDto = _mapper.Map<RoleDto>(newRole);
            return (true, "Role created successfully", roleDto);
        }

        public async Task<(bool Success, string Message)> DeleteRoleAsync(int roleId)
        {
            var existingRole = await _unitOfWork.RoleRepository.GetByIdAsync(roleId);
            if (existingRole == null)
                return (false, "Role code doesn't exists");

            await _unitOfWork.RoleRepository.RemoveAsync(existingRole);
            return (true, "Role deleted successfully");
        }

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _unitOfWork.RoleRepository.GetAllAsync();
            var roleDto = _mapper.Map<List<RoleDto>>(roles);
            return roleDto;
        }

        public async Task<RoleDto?> GetRoleByNameAsync(string roleName)
        {
            var role = await _unitOfWork.RoleRepository.GetRoleByNameAsync(roleName);
            var roleDto = _mapper.Map<RoleDto>(role);
            return roleDto;
        }

        public async Task<(bool Success, string Message)> UpdateRoleAsync(RoleDto role)
        {
            var existRole = await _unitOfWork.RoleRepository.GetByIdAsync(role.Id);
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
