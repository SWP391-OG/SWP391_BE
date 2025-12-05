using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts;
using SWP391.Contracts.Authentication;
using SWP391.Contracts.Common;
using SWP391.Contracts.Department;
using SWP391.Contracts.Location;
using SWP391.Repositories.Models;
using SWP391.Services.Application;
using SWP391.WebAPI.Constants;

namespace SWP391.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IApplicationServices _applicationServices;

        public DepartmentController(IApplicationServices applicationServices)
        {
            _applicationServices = applicationServices;
        }

        /// <summary>
        /// Get all department 
        /// </summary>
        [HttpGet("/api/Departments")]
        [ProducesResponseType(typeof(ApiResponse<List<DepartmentDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        public async Task<IActionResult> GetAllDepartmentCode()
        {
            var departments = await _applicationServices.DepartmentService.GetAllDepartmentsAsync();
            
            if (departments == null || !departments.Any())
            {
                return NotFound(ApiResponse<object>.ErrorResponse("No department found"));
            }

            return Ok(ApiResponse<List<DepartmentDto>>.SuccessResponse(departments, "Department retrieved successfully"));
           
        }

        /// <summary>
        /// Get department by code
        /// </summary>
        [HttpGet("{departmentCode}")]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        public async Task<IActionResult> GetByDepartmentCode(string departmentCode)
        {
            var department = await _applicationServices.DepartmentService.GetByDepartmentCodeAsync(departmentCode);
            if (department == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("No department found"));
            }
            return Ok(ApiResponse<DepartmentDto>.SuccessResponse(department, "Department retrieved successfully"));
        }

        /// <summary>
        /// Create department 
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> CreateDepartment(DepartmentRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message, data) = await _applicationServices
                 .DepartmentService.CreateDepartmentAsync(dto);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<DepartmentDto>.SuccessResponse(data, message));
        }


        /// <summary>
        /// Update department 
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> UpdateDepartment([FromBody] DepartmentRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message) = await _applicationServices.DepartmentService.UpdateDepartmentAsync(dto);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<LocationDto>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Update department status
        /// </summary>
        [HttpPatch("status")]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> UpdateLocationStatus([FromBody] DepartmentStatusUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }
            var (success, message) = await _applicationServices
                .DepartmentService.UpdateStatusDepartmentAsync(dto);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<DepartmentDto>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Update department status
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> DeleteDepartmentByCode([FromQuery] string departmentCode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }
            var (success, message) = await _applicationServices
                .DepartmentService.DeleteDepartmentByCodeAsync(departmentCode);
            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }
            return Ok(ApiResponse<DepartmentDto>.SuccessResponse(null, message));
        }
    }
}
