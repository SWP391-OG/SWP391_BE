using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts.Common;
using SWP391.Contracts.Department;
using SWP391.Services.Application;
using SWP391.WebAPI.Constants;
using System.Security.Claims;

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
        /// Get all departments 
        /// </summary>
        /// <param >Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated tickets.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpGet("/api/Departments")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DepartmentDto>>), ApiStatusCode.OK)]
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
        /// <param >Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated tickets.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpGet("{departmentCode}")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DepartmentDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
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
        /// <param >Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated tickets.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DepartmentDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize(Roles = "Admin")]
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
        /// <param >Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated tickets.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpPut("{departmentId}")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DepartmentDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDepartment(int departmentId, [FromBody] DepartmentRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message) = await _applicationServices.DepartmentService.UpdateDepartmentAsync(departmentId, dto);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

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
        /// Delete department
        /// </summary>
        /// <param >Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated tickets.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpPatch("status")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<DepartmentDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatusDepartment(DepartmentStatusUpdateDto dto)
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
    }
}
