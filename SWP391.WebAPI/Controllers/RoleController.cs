using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts;
using SWP391.Contracts.Common;
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
        /// <response code="200">Returns all roles.</response>
        /// <response code="404">No roles found.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<Role>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
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
        /// Get roles by name
        /// </summary>
        /// <param name="roleName">The role code to search for</param>
        /// <response code="200">Returns the role.</response>
        /// <response code="404">Role not found.</response>
        [HttpGet("{roleName}")]
        [ProducesResponseType(typeof(ApiResponse<Role>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
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
        /// Create a new role
        /// </summary>
        /// <param name="dto">Role creation data</param>
        /// <response code="201">Role created successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Role>), ApiStatusCode.CREATED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
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
        /// Update an existing Role
        /// </summary>
        /// <param name="role">Role update data</param>
        /// <response code="200">Role updated successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="404">Role not found.</response>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
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
        /// Delete a role by code
        /// </summary>
        /// <param name="categoryCode">The role code to delete</param>
        /// <response code="200">Role deleted successfully.</response>
        /// <response code="400">Invalid request or business rule violation.</response>
        /// <response code="404">Role not found.</response>
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        public async Task<IActionResult> DeleteRole([FromQuery] int roleId)
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
