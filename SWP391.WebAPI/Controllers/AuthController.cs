using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts.Authentication;
using SWP391.Contracts.Common;
using SWP391.Services.Application;
using SWP391.WebAPI.Constants;

namespace SWP391.WebAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IApplicationServices _applicationServices;

        public AuthController(IApplicationServices applicationServices)
        {
            _applicationServices = applicationServices;
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.CREATED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message) = await _applicationServices.AuthenticationService.RegisterAsync(request);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return StatusCode(ApiStatusCode.CREATED, ApiResponse<object>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Verify a user's email address using a verification code
        /// </summary>
        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message) = await _applicationServices.AuthenticationService.VerifyEmailAsync(request);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Resend the email verification code to a user
        /// </summary>
        [HttpPost("resend-verification")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message) = await _applicationServices.AuthenticationService.ResendVerificationAsync(request);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Request a password reset email
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message) = await _applicationServices.AuthenticationService.ForgotPasswordAsync(request);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Reset a user's password using a reset code
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message) = await _applicationServices.AuthenticationService.ResetPasswordAsync(request);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Log in with email and password
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var (success, message, data) = await _applicationServices.AuthenticationService.LoginAsync(request);

            if (!success)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(data, message));
        }
    }
}