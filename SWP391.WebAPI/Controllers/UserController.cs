using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts.Common;
using SWP391.Contracts.User;
using SWP391.Services.Application;
using SWP391.WebAPI.Constants;
using System.Security.Claims;

namespace SWP391.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UserController : ControllerBase
    {
        private readonly IApplicationServices _applicationServices;

        public UserController(IApplicationServices applicationServices)
        {
            _applicationServices = applicationServices;
        }

        #region Helper Methods

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }

        private IActionResult HandleAuthenticationError()
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse(ApiMessages.INVALID_USER_AUTHENTICATION));
        }

        #endregion

        /// <summary>
        /// Get all users 
        /// </summary>
        /// <response code="200">Returns all users.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can access this.</response>
        /// <response code="404">No users found.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _applicationServices.UserService.GetAllUsersAsync();

            if (users == null || !users.Any())
            {
                return NotFound(ApiResponse<object>.ErrorResponse("No users found"));
            }

            return Ok(ApiResponse<List<UserDto>>.SuccessResponse(users, "Users retrieved successfully"));
        }

        /// <summary>
        /// Get user profile
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <response code="200">Returns the user.</response>
        /// <response code="400">Invalid user ID.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can access this.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize(Roles = "Student,Staff")]
        public async Task<IActionResult> GetProfileByUseCode()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _applicationServices.UserService.GetUseProfileByUserIdAsync(int.Parse(userIdClaim));

            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
            }

            return Ok(ApiResponse<UserProfileDto>.SuccessResponse(user, "User retrieved successfully"));
        }

        /// <summary>
        /// Update user profile  
        /// </summary>
        /// <param name="userDto">The user ID</param>
        /// <response code="200">Returns the user.</response>
        /// <response code="400">Invalid user ID.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can access this.</response>
        /// <response code="404">User not found.</response>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize(Roles = "Student,Staff")]
        public async Task<IActionResult> UpdateUserProfile(UserUpdateProfileDto userDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var (success, message) = await _applicationServices.UserService.UpdateProfileUserAsync(int.Parse(userIdClaim),userDto);

            if (!success)
            {
                if (message == "User not found")
                    return NotFound(ApiResponse<object>.ErrorResponse(message));

                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }


            return Ok(ApiResponse<UserProfileDto>.SuccessResponse(null, "User retrieved successfully"));
        }

        /// <summary>
        /// Create a new user 
        /// </summary>
        /// <param name="userDto">User creation data</param>
        /// <response code="201">User created successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can create users.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), ApiStatusCode.CREATED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid request data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message, data) = await _applicationServices.UserService.CreateUserAsync(userDto);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return StatusCode(ApiStatusCode.CREATED, ApiResponse<UserDto>.SuccessResponse(data, message));
        }

        /// <summary>
        /// Update an existing user 
        /// </summary>
        /// <param name="userCode">The user ID to update</param>
        /// <param name="userDto">User update data</param>
        /// <response code="200">User updated successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can update users.</response>
        /// <response code="404">User not found.</response>
        [HttpPut("{userCode}")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize(Roles = "Admin")]    
        public async Task<IActionResult> UpdateUser( string userCode,[FromBody] UserUpdateDto userDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid request data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message) = await _applicationServices.UserService.UpdateUserAsync(userCode, userDto);

            if (!success)
            {
                if (message == "User not found")
                    return NotFound(ApiResponse<object>.ErrorResponse(message));

                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Delete a user (soft delete - sets status to Inactive) (Admin only)
        /// </summary>
        /// <param name="id">The user ID to delete</param>
        /// <response code="200">User deleted successfully.</response>
        /// <response code="400">Invalid user ID or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can delete users.</response>
        /// <response code="404">User not found.</response>
        [HttpDelete]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string code)
        {
            if (code == null)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid user code"));
            }

            var (success, message) = await _applicationServices.UserService.DeleteUserAsync(code);

            if (!success)
            {
                if (message == "User not found")
                    return NotFound(ApiResponse<object>.ErrorResponse(message));

                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }
    }
}
