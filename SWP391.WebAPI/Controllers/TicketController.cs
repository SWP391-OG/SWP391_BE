using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts.Common;
using SWP391.Contracts.Ticket;
using SWP391.Services.Application;
using SWP391.WebAPI.Constants;
using System.Security.Claims;

namespace SWP391.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all endpoints
    public class TicketController : ControllerBase
    {
        private readonly IApplicationServices _applicationServices;

        public TicketController(IApplicationServices applicationServices)
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

        #region GET - Query Endpoints

        /// <summary>
        /// Get all tickets with pagination, search, and filtering (Admin only)
        /// </summary>
        /// <param name="request">Search and pagination parameters (query string)</param>
        /// <response code="200">Returns paginated tickets.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Insufficient permissions.</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<TicketDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        public async Task<IActionResult> GetAllTickets([FromQuery] TicketSearchRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var paginatedTickets = await _applicationServices.TicketService.GetAllTicketsWithPaginationAsync(request);

            return Ok(ApiResponse<PaginatedResponse<TicketDto>>.SuccessResponse(
                paginatedTickets,
                $"Retrieved {paginatedTickets.Items.Count} tickets (Page {paginatedTickets.PageNumber} of {paginatedTickets.TotalPages})"));
        }

        /// <summary>
        /// Get ticket by code (All authenticated users)
        /// </summary>
        /// <param name="ticketCode">The ticket code</param>
        /// <response code="200">Returns the ticket.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="404">Ticket not found.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        [HttpGet("{ticketCode}")]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        public async Task<IActionResult> GetTicketByCode(string ticketCode)
        {
            if (string.IsNullOrWhiteSpace(ticketCode))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ApiMessages.INVALID_REQUEST_DATA));
            }

            var ticket = await _applicationServices.TicketService.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ApiMessages.TICKET_NOT_FOUND));
            }

            return Ok(ApiResponse<TicketDto>.SuccessResponse(ticket, ApiMessages.TICKET_RETRIEVED_SUCCESS));
        }

        /// <summary>
        /// Get all tickets created by the current student (with pagination and search)
        /// </summary>
        /// <param name="request">Search and pagination parameters (query string)</param>
        /// <response code="200">Returns student's tickets.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only students can access this.</response>
        [HttpGet("my-tickets")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<TicketDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        public async Task<IActionResult> GetMyTickets([FromQuery] PaginationRequestDto request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleAuthenticationError();
            }

            var paginatedTickets = await _applicationServices.TicketService.GetMyTicketsWithPaginationAsync(userId.Value, request);

            return Ok(ApiResponse<PaginatedResponse<TicketDto>>.SuccessResponse(
                paginatedTickets,
                $"Retrieved {paginatedTickets.Items.Count} tickets (Page {paginatedTickets.PageNumber} of {paginatedTickets.TotalPages})"));
        }

        /// <summary>
        /// Get all tickets assigned to the current staff (with pagination and search)
        /// </summary>
        /// <param name="request">Search and pagination parameters (query string)</param>
        /// <response code="200">Returns staff's assigned tickets.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only staff can access this.</response>
        [HttpGet("my-assigned-tickets")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<TicketDto>>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        public async Task<IActionResult> GetMyAssignedTickets([FromQuery] PaginationRequestDto request)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleAuthenticationError();
            }

            var paginatedTickets = await _applicationServices.TicketService.GetMyAssignedTicketsWithPaginationAsync(userId.Value, request);

            return Ok(ApiResponse<PaginatedResponse<TicketDto>>.SuccessResponse(
                paginatedTickets,
                $"Retrieved {paginatedTickets.Items.Count} tickets (Page {paginatedTickets.PageNumber} of {paginatedTickets.TotalPages})"));
        }

        #endregion

        #region POST - Create Operations

        /// <summary>
        /// Student creates a new ticket
        /// </summary>
        /// <param name="dto">Ticket creation data</param>
        /// <response code="201">Ticket created successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only students can create tickets.</response>
        [HttpPost]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), ApiStatusCode.CREATED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleAuthenticationError();
            }

            var (success, message, data) = await _applicationServices
                .TicketService.CreateTicketAsync(dto, userId.Value);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return StatusCode(ApiStatusCode.CREATED, ApiResponse<TicketDto>.SuccessResponse(data, message));
        }

        #endregion

        #region PUT - Update Operations

        /// <summary>
        /// Student updates their own NEW ticket
        /// </summary>
        /// <param name="ticketCode">The ticket code</param>
        /// <param name="dto">Ticket update data</param>
        /// <response code="200">Ticket updated successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only ticket owner can update.</response>
        /// <response code="404">Ticket not found.</response>
        [HttpPut("{ticketCode}")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        public async Task<IActionResult> UpdateTicket(string ticketCode, [FromBody] UpdateTicketDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            if (string.IsNullOrWhiteSpace(ticketCode))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ApiMessages.INVALID_REQUEST_DATA));
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleAuthenticationError();
            }

            var (success, message) = await _applicationServices
                .TicketService.UpdateTicketAsync(ticketCode, dto, userId.Value);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        #endregion

        #region PATCH - Status & Workflow Operations

        /// <summary>
        /// Staff updates ticket status (ASSIGNED → IN_PROGRESS → RESOLVED)
        /// </summary>
        /// <param name="ticketCode">The ticket code</param>
        /// <param name="dto">Status update data</param>
        /// <response code="200">Status updated successfully.</response>
        /// <response code="400">Invalid status transition or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only staff can update status.</response>
        [HttpPatch("{ticketCode}/status")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        public async Task<IActionResult> UpdateTicketStatus(string ticketCode, [FromBody] UpdateTicketStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            if (string.IsNullOrWhiteSpace(ticketCode))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ApiMessages.INVALID_REQUEST_DATA));
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleAuthenticationError();
            }

            var (success, message) = await _applicationServices
                .TicketService.UpdateTicketStatusAsync(ticketCode, dto, userId.Value);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Admin assigns ticket AUTOMATICALLY (finds staff with least workload)
        /// </summary>
        /// <param name="ticketCode">The ticket code to assign</param>
        /// <response code="200">Ticket assigned automatically.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can assign tickets.</response>
        [HttpPatch("{ticketCode}/assign")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<string>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        public async Task<IActionResult> AssignTicketAutomatically(string ticketCode)
        {
            if (string.IsNullOrWhiteSpace(ticketCode))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ApiMessages.INVALID_REQUEST_DATA));
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleAuthenticationError();
            }

            var (success, message, assignedStaffCode) = await _applicationServices
                .TicketService.AssignTicketAutomaticallyAsync(ticketCode, userId.Value);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<string>.SuccessResponse(assignedStaffCode, message));
        }

        /// <summary>
        /// Admin assigns ticket MANUALLY to a specific staff
        /// </summary>
        /// <param name="ticketCode">The ticket code to assign</param>
        /// <param name="dto">Manual assignment data</param>
        /// <response code="200">Ticket assigned manually.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can assign tickets.</response>
        [HttpPatch("{ticketCode}/assign/manual")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        public async Task<IActionResult> AssignTicketManually(string ticketCode, [FromBody] AssignTicketRequestDto dto)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.ManualStaffCode))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ApiMessages.STAFF_CODE_REQUIRED));
            }

            if (string.IsNullOrWhiteSpace(ticketCode))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ApiMessages.INVALID_REQUEST_DATA));
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleAuthenticationError();
            }

            var (success, message) = await _applicationServices
                .TicketService.AssignTicketManuallyAsync(ticketCode, dto.ManualStaffCode, userId.Value);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Student provides feedback for a RESOLVED ticket
        /// </summary>
        /// <param name="ticketCode">The ticket code</param>
        /// <param name="dto">Feedback data</param>
        /// <response code="200">Feedback submitted successfully, ticket closed.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only students can provide feedback.</response>
        [HttpPatch("{ticketCode}/feedback")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        public async Task<IActionResult> ProvideFeedback(string ticketCode, [FromBody] TicketFeedbackDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            if (string.IsNullOrWhiteSpace(ticketCode))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ApiMessages.INVALID_REQUEST_DATA));
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleAuthenticationError();
            }

            var (success, message) = await _applicationServices
                .TicketService.ProvideFeedbackAsync(ticketCode, dto, userId.Value);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        #endregion

        #region DELETE - Soft Delete Operations

        /// <summary>
        /// Student cancels their own NEW ticket (SOFT DELETE - sets status to CANCELLED)
        /// </summary>
        /// <param name="ticketCode">The ticket code to cancel</param>
        /// <param name="dto">Cancellation request with reason</param>
        /// <response code="200">Ticket cancelled successfully.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only ticket owner can cancel NEW tickets.</response>
        /// <response code="404">Ticket not found.</response>
        [HttpDelete("{ticketCode}")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        public async Task<IActionResult> CancelTicket(string ticketCode, [FromBody] CancelTicketRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            if (string.IsNullOrWhiteSpace(ticketCode))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ApiMessages.INVALID_REQUEST_DATA));
            }

            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Cancellation reason is required"));
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleAuthenticationError();
            }

            var (success, message) = await _applicationServices
                .TicketService.CancelTicketAsync(ticketCode, userId.Value, dto.Reason);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        /// <summary>
        /// Admin cancels any ticket (SOFT DELETE - sets status to CANCELLED)
        /// </summary>
        /// <param name="ticketCode">The ticket code to cancel</param>
        /// <param name="dto">Cancellation request with reason</param>
        /// <response code="200">Ticket cancelled successfully by administrator.</response>
        /// <response code="400">Invalid request data or business rule violation.</response>
        /// <response code="401">Unauthorized - Invalid authentication.</response>
        /// <response code="403">Forbidden - Only admins can force cancel tickets.</response>
        /// <response code="404">Ticket not found.</response>
        [HttpDelete("{ticketCode}/cancel")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.UNAUTHORIZED)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.FORBIDDEN)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        public async Task<IActionResult> AdminCancelTicket(string ticketCode, [FromBody] CancelTicketRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    ApiMessages.INVALID_REQUEST_DATA,
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            if (string.IsNullOrWhiteSpace(ticketCode))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ApiMessages.INVALID_REQUEST_DATA));
            }

            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Cancellation reason is required"));
            }

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return HandleAuthenticationError();
            }

            var (success, message) = await _applicationServices
                .TicketService.AdminCancelTicketAsync(ticketCode, userId.Value, dto.Reason);

            if (!success)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(message));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        #endregion

        #region Overdue & Duplicate Detection

        /// <summary>
        /// Get all overdue tickets (Admin only)
        /// </summary>
        [HttpGet("overdue")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<List<TicketDto>>), ApiStatusCode.OK)]
        public async Task<IActionResult> GetOverdueTickets()
        {
            var tickets = await _applicationServices.TicketService.GetOverdueTicketsAsync();
            return Ok(ApiResponse<List<TicketDto>>.SuccessResponse(tickets, ApiMessages.OVERDUE_TICKETS_RETRIEVED));
        }

        /// <summary>
        /// Get my overdue tickets (Staff only)
        /// </summary>
        [HttpGet("my-overdue-tickets")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(ApiResponse<List<TicketDto>>), ApiStatusCode.OK)]
        public async Task<IActionResult> GetMyOverdueTickets()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return HandleAuthenticationError();

            var tickets = await _applicationServices.TicketService.GetOverdueTicketsByStaffIdAsync(userId.Value);
            return Ok(ApiResponse<List<TicketDto>>.SuccessResponse(tickets, ApiMessages.OVERDUE_TICKETS_RETRIEVED));
        }

        /// <summary>
        /// Escalate a ticket (Admin only)
        /// </summary>
        [HttpPatch("{ticketCode}/escalate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        public async Task<IActionResult> EscalateTicket(string ticketCode)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return HandleAuthenticationError();

            var (success, message) = await _applicationServices.TicketService
                .EscalateTicketAsync(ticketCode, userId.Value);

            if (!success)
                return BadRequest(ApiResponse<object>.ErrorResponse(message));

            return Ok(ApiResponse<object>.SuccessResponse(null, message));
        }

        #endregion
    }
}