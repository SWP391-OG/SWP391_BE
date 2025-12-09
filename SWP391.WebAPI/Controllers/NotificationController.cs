using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts.Common;
using SWP391.Contracts.Notification;
using SWP391.Services.Application;
using SWP391.WebAPI.Constants;
using System.Security.Claims;

namespace SWP391.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly IApplicationServices _applicationServices;

        public NotificationController(IApplicationServices applicationServices)
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
        /// Get current user's notifications with pagination
        /// </summary>
        /// <param name="request">Pagination parameters (query string)</param>
        /// <response code="200">Returns paginated notifications.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        [HttpGet("my-notifications")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<NotificationDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        public async Task<IActionResult> GetMyNotifications([FromQuery] NotificationPaginationRequestDto request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleAuthenticationError();
            }

            var paginatedNotifications = await _applicationServices.NotificationService
                .GetMyNotificationsAsync(userId.Value, request);

            return Ok(ApiResponse<PaginatedResponse<NotificationDto>>.SuccessResponse(
                paginatedNotifications,
                $"Retrieved {paginatedNotifications.Items.Count} notifications (Page {paginatedNotifications.PageNumber} of {paginatedNotifications.TotalPages})"));
        }

        /// <summary>
        /// Get count of unread notifications for current user
        /// </summary>
        /// <response code="200">Returns unread notification count.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ApiResponse<int>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleAuthenticationError();
            }

            var unreadCount = await _applicationServices.NotificationService.GetUnreadCountAsync(userId.Value);

            return Ok(ApiResponse<int>.SuccessResponse(unreadCount, ApiMessages.UNREAD_COUNT_RETRIEVED));
        }

        /// <summary>
        /// Mark a specific notification as read
        /// </summary>
        /// <param name="notificationId">The notification ID to mark as read</param>
        /// <response code="200">Notification marked as read.</response>
        /// <response code="400">Invalid request or notification not found.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        [HttpPatch("{notificationId}/mark-read")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleAuthenticationError();
            }

            var (success, message) = await _applicationServices.NotificationService
                .MarkAsReadAsync(notificationId, userId.Value);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Mark all notifications as read for current user
        /// </summary>
        /// <response code="200">All notifications marked as read.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        [HttpPatch("mark-all-read")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleAuthenticationError();
            }

            var (success, message) = await _applicationServices.NotificationService.MarkAllAsReadAsync(userId.Value);

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }
    }
}