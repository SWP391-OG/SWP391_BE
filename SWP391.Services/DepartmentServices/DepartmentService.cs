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
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<(bool Success, string Message, DepartmentDto Data)> CreateDepartmentAsync(DepartmentRequestDto dto)
        {
            var existingCode = await _unitOfWork.DepartmentRepository.GetDepartmentByCodeAsync(dto.DeptCode);
            if (existingCode != null)
                return (false, "Location code already exists", null);

            var existingName = await _unitOfWork.DepartmentRepository.GetDepartmentByNameAsync(dto.DeptName);
            if (existingName != null)
                return (false, "Location name already exists", null);

            var department = _mapper.Map<Department>(dto);
            department.CreatedAt = DateTime.Now;
            await _unitOfWork.DepartmentRepository.CreateAsync(department);
            var deparmentDto = _mapper.Map<DepartmentDto>(department);
            return (true, "Department created successfully", deparmentDto);
        }

        public async Task<(bool Success, string Message)> DeleteDepartmentByCodeAsync(string departmentCode)
        {
            var existingCode = await _unitOfWork.DepartmentRepository.GetDepartmentByCodeAsync(departmentCode);
            if (existingCode == null)
                return (false, "Department code doesn't exists");
            await _unitOfWork.DepartmentRepository.RemoveAsync(existingCode);
            return (true, "Department deleted successfully");
        }



        public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();
            var exis = new List<DepartmentDto>();
            DepartmentDto existLocation;
            foreach (var department in departments)
            {
                existLocation = _mapper.Map<DepartmentDto>(department);
                exis.Add(existLocation);
            }

            return exis;
        }

        public async Task<DepartmentDto> GetByDepartmentCodeAsync(string departmentCode)
            => await _unitOfWork.DepartmentRepository.GetDepartmentByCodeAsync(departmentCode)
                .ContinueWith(task => _mapper.Map<DepartmentDto>(task.Result));

        public async Task<(bool Success, string Message)> UpdateDepartmentAsync(DepartmentRequestDto dto)
        {
            var department = await _unitOfWork.DepartmentRepository.GetDepartmentByCodeAsync(dto.DeptCode);
            if (department == null)
            {
                return (false, "Department not found");
            }
            department.DeptCode = dto.DeptCode;
            department.DeptCode = dto.DeptName;
            _unitOfWork.DepartmentRepository.Update(department);

            return (true, "Department updated successfully");
        }



        public async Task<(bool Success, string Message)> UpdateStatusDepartmentAsync(DepartmentStatusUpdateDto dto)
        {
            var department = await _unitOfWork.DepartmentRepository.GetDepartmentByCodeAsync(dto.DeptCode);


            if (department == null)
            {
                return (false, "Department not found");
            }
            department.Status = dto.Status;
            _unitOfWork.DepartmentRepository.Update(department);

            return (true, "Department status updated successfully");
        }

    }
}
