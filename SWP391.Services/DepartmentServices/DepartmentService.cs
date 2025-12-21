using AutoMapper;
using SWP391.Contracts.Department;
using SWP391.Contracts.Location;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Services.DepartmentServices
{
    /// <summary>
    /// Service for managing department operations
    /// </summary>
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Create a new department
        /// </summary>
        public async Task<(bool Success, string Message, DepartmentDto Data)> CreateDepartmentAsync(DepartmentRequestDto dto)
        {
            if (dto == null)
                return (false, "Department data cannot be null", null);

            if (string.IsNullOrWhiteSpace(dto.DeptCode))            
                return (false, "Department code is required", null);
            
            if (string.IsNullOrWhiteSpace(dto.DeptName))           
                return (false, "Department name is required", null);            

            var existingCode = await _unitOfWork.DepartmentRepository.GetDepartmentByCodeAsync(dto.DeptCode);
            if (existingCode != null)
                return (false, "Department code already exists", null);

            var existingName = await _unitOfWork.DepartmentRepository.GetDepartmentByNameAsync(dto.DeptName);
            if (existingName != null)
                return (false, "Department name already exists", null);

            var department = _mapper.Map<Department>(dto);
            department.Status = "ACTIVE";
            department.CreatedAt = DateTime.Now;

            await _unitOfWork.DepartmentRepository.CreateAsync(department);

            var deparmentDto = _mapper.Map<DepartmentDto>(department);
            return (true, "Department created successfully", deparmentDto);
        }

         /// <summary>
        /// Delete department (soft delete by marking as inactive)
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteDepartmentAsync(int departmentId)
        {
             if (departmentId <= 0)
                return (false, "Invalid department ID");

            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
            if (department == null)
                return (false, "Department code doesn't exists");
            department.Status = "INACTIVE";

             _unitOfWork.DepartmentRepository.Update(department);
            return (true, "Department deleted successfully");
        }


        /// <summary>
        /// Get all departments (active and inactive)
        /// </summary>
        public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
            return _mapper.Map<List<DepartmentDto>>(departments);
        }

         /// <summary>
        /// Get department by department code
        /// </summary>
        public async Task<DepartmentDto> GetByDepartmentCodeAsync(string departmentCode)
          {
            if (string.IsNullOrWhiteSpace(departmentCode))
                return null;

            var department = await _unitOfWork.DepartmentRepository.GetDepartmentByCodeAsync(departmentCode);
            return department == null ? null : _mapper.Map<DepartmentDto>(department);
        }

        /// <summary>
        /// Update department information
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateDepartmentAsync(int departmentId, DepartmentRequestDto dto)
        {
            if (departmentId <= 0)
                return (false, "Invalid department ID");

            if (dto == null)
                return (false, "Department data cannot be null");

            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(departmentId);
            if (department == null)            
                return (false, "Department not found");

            if (string.IsNullOrWhiteSpace(dto.DeptCode))           
                return (false, "Department code is required");
            
            if (string.IsNullOrWhiteSpace(dto.DeptName))          
                return (false, "Department name is required");
            
            department.DeptCode = dto.DeptCode;
            department.DeptName = dto.DeptName;

            await _unitOfWork.DepartmentRepository.UpdateAsync(department);

            return (true, "Department updated successfully");
        }

        /// <summary>
        /// Update department status
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateStatusDepartmentAsync(DepartmentStatusUpdateDto dto)
        {
            if (dto == null)
                return (false, "Status update request cannot be null");

            if (dto.DepartmentId <= 0)
                return (false, "Invalid department ID");

            if (string.IsNullOrWhiteSpace(dto.Status))
                return (false, "Status cannot be empty");

            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(dto.DepartmentId);

            if (department == null)           
                return (false, "Department not found");
            
            department.Status = dto.Status;
            await _unitOfWork.DepartmentRepository.UpdateAsync(department);

            return (true, "Department status updated successfully");
        }

         /// <summary>
        /// Get all active departments
        /// </summary>
        public async Task<List<DepartmentDto>> GetAllActiveDepartmentAsync()
        {
            var departments = await _unitOfWork.DepartmentRepository.GetAllActiveDepartmentAsync();
            return _mapper.Map<List<DepartmentDto>>(departments);
        }

    }
}
