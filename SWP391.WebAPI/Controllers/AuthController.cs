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
        /// <remarks>
        /// Creates a new user account and sends a verification email with a 6-digit code.
        /// The verification code expires in 15 minutes.
        /// </remarks>
        /// <response code="201">User registered successfully. Verification email sent.</response>
        /// <response code="400">Invalid request data or email already exists.</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.CREATED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
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
        /// <remarks>
        /// Activates the user account by verifying the 6-digit code sent via email.
        /// The code must be used within 15 minutes of generation.
        /// </remarks>
        /// <response code="200">Email verified successfully. User can now log in.</response>
        /// <response code="400">Invalid verification code, expired code, or user not found.</response>
        [HttpPost("verify-email")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
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
        /// <remarks>
        /// Invalidates all previous verification codes and sends a new 6-digit code.
        /// Can only be used for accounts that haven't been verified yet.
        /// </remarks>
        /// <response code="200">New verification code sent successfully.</response>
        /// <response code="400">User not found, email already verified, or invalid request.</response>
        [HttpPost("resend-verification")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
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
        /// <remarks>
        /// Sends a password reset code to the user's email if the account exists and is active.
        /// For security, the response doesn't indicate whether the email exists.
        /// The reset code expires in 15 minutes.
        /// </remarks>
        /// <response code="200">Password reset email sent if account exists.</response>
        /// <response code="400">Invalid request data or account not active.</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
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
        /// <remarks>
        /// Resets the user's password using the code sent via email.
        /// The reset code must be used within 15 minutes and can only be used once.
        /// </remarks>
        /// <response code="200">Password reset successfully. User can now log in with new password.</response>
        /// <response code="400">Invalid reset code, expired code, or user not found.</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
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
        /// <remarks>
        /// Authenticates a user and returns a JWT token for authorized requests.
        /// The account must be verified (active status) before logging in.
        /// </remarks>
        /// <response code="200">Login successful. Returns JWT token and user information.</response>
        /// <response code="400">Invalid request data (validation errors).</response>
        /// <response code="401">Invalid credentials or account not verified.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
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