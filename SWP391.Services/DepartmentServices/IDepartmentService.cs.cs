using SWP391.Contracts.Department;
using SWP391.Contracts.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Services.DepartmentServices
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto> GetByDepartmentCodeAsync(string departmentCode);
        Task<(bool Success, string Message, DepartmentDto Data)> CreateDepartmentAsync(DepartmentRequestDto dto);
        Task<(bool Success, string Message)> UpdateDepartmentAsync(int departmentId,DepartmentRequestDto dto);
        Task<(bool Success, string Message)> UpdateStatusDepartmentAsync(DepartmentStatusUpdateDto dto);
        Task<(bool Success, string Message)> DeleteDepartmentByCodeAsync(string departmentCode);
    }
}
