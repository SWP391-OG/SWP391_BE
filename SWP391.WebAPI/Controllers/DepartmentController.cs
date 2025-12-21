using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts.Common;
using SWP391.Contracts.Department;
using SWP391.Services.Application;
using SWP391.WebAPI.Constants;
using System.Security.Claims;

namespace SWP391.WebAPI.Controllers
{
    /// <summary>
    /// API controller for managing departments
    /// </summary>
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
        /// Get all departments (admin sees all, others see active only)
        /// </summary>
        /// <response code="200">Returns all departments.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        /// <response code="404">No departments found.</response>
        [HttpGet("/api/Departments")]
        [ProducesResponseType(typeof(ApiResponse<List<DepartmentDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize]
        public async Task<IActionResult> GetAllDepartmentCode()
        {
            var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            var departments = new List<DepartmentDto>();

            if (userRoleClaim == "Admin")
            {
                departments = await _applicationServices.DepartmentService.GetAllDepartmentsAsync();
                if (departments == null || !departments.Any())
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("No locations found"));
                }
            }
            else
            {
                departments = await _applicationServices.DepartmentService.GetAllActiveDepartmentAsync();
                if (departments == null || !departments.Any())
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("No locations found"));
                }
            }

            return Ok(ApiResponse<List<DepartmentDto>>.SuccessResponse(departments, "Department retrieved successfully"));

        }

        /// <summary>
        /// Get department by code
        /// </summary>
        /// <param name="departmentCode">The department code</param>
        /// <response code="200">Returns the department.</response>
        /// <response code="400">Invalid department code.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="404">Department not found.</response>
        [HttpGet("{departmentCode}")]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize]
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
        /// <param name="dto">Department creation data</param>
        /// <response code="201">Department created successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can create departments.</response>
        /// <response code="404">Department not found.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDepartment(DepartmentRequestDto dto)
        {
             if (dto == null)
                return BadRequest(ApiResponse<object>.ErrorResponse("Department data is required"));

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
        /// <param name="departmentId">Department ID</param>
        /// <param name="dto">Department update data</param>
        /// <response code="200">Department updated successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can update departments.</response>
        /// <response code="404">Department not found.</response>
        [HttpPut("{departmentId}")]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDepartment(int departmentId, [FromBody] DepartmentRequestDto dto)
        {
             if (departmentId <= 0)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid department ID"));

            if (dto == null)
                return BadRequest(ApiResponse<object>.ErrorResponse("Department data is required"));

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message) = await _applicationServices.DepartmentService.UpdateDepartmentAsync(departmentId, dto);

            if (!success)           
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            
            return Ok(ApiResponse<DepartmentDto>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Update department status
        /// </summary>
        /// <param >Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated tickets.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        //[HttpDelete("{departmentId}")]
        //[ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DepartmentDto>>), ApiStatusCode.OK)]
        //[ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        //[ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        //[ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        //[Authorize(Roles = "Admin")]
        //public async Task<IActionResult> DeleteDepartmentAsync(int departmentId)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ApiResponse<object>.ErrorResponse(
        //            ApiMessages.INVALID_REQUEST_DATA,
        //            ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
        //    }
        //    var (success, message) = await _applicationServices
        //        .DepartmentService.DeleteDepartmentAsync(departmentId);

        //    if (!success)
        //    {
        //        return BadRequest(ApiResponse<object>.ErrorResponse(message));
        //    }

        //    return Ok(ApiResponse<DepartmentDto>.SuccessResponse(null, message));
        //}

        /// <summary>
        /// Update department status (ACTIVE or INACTIVE)
        /// </summary>
        /// <param name="dto">Status update data</param>
        /// <response code="200">Department status updated successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can update status.</response>
        /// <response code="404">Department not found.</response>
        [HttpPatch("status")]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatusDepartment(DepartmentStatusUpdateDto dto)
        {
             if (dto == null)
                return BadRequest(ApiResponse<object>.ErrorResponse("Status update data is required"));

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
    }
}
