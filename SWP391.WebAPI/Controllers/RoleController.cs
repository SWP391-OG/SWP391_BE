using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts.Common;
using SWP391.Contracts.Role;
using SWP391.Contracts.Ticket;
using SWP391.Repositories.Models;
using SWP391.Services.Application;
using SWP391.WebAPI.Constants;

namespace SWP391.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IApplicationServices _applicationServices;

        public RoleController(IApplicationServices applicationServices)
        {
            _applicationServices = applicationServices;
        }


        /// <summary>
        /// Get all roles 
        /// </summary>
        /// <param >Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated role.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<RoleDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllRole()
        {
            var roles = await _applicationServices.RoleService.GetAllRolesAsync();

            if (roles == null || !roles.Any())
            {
                return NotFound(ApiResponse<object>.ErrorResponse("No roles found"));
            }

            return Ok(ApiResponse<List<RoleDto>>.SuccessResponse(roles, "Roles retrieved successfully"));
        }

        /// <summary>
        /// Get by role name 
        /// </summary>
        /// <param name="roleName" >Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated role.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpGet("{roleName}")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByRoleName(string roleName)
        {
            var role = await _applicationServices.RoleService.GetRoleByNameAsync(roleName);

            if (role == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Role with code '{roleName}' not found"));
            }

            return Ok(ApiResponse<RoleDto>.SuccessResponse(role, "Role retrieved successfully"));
        }

        /// <summary>
        /// Create role 
        /// </summary>
        /// <param name="roleName" >Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated role.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message, data) = await _applicationServices
                .RoleService.CreateRoleAsync(roleName);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return StatusCode(ApiStatusCode.CREATED, ApiResponse<RoleDto>.SuccessResponse(data, message));
        }

        /// <summary>
        /// Update role
        /// </summary>
        /// <param name="role" >Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated role.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole([FromBody] RoleDto role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message) = await _applicationServices
                .RoleService.UpdateRoleAsync(role);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Delete role
        /// </summary>
        /// <param name="roleId" >Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated role.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpDelete("{roleId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(int roleId)
        {
            var (success, message) = await _applicationServices
                .RoleService.DeleteRoleAsync(roleId);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

    }
}
