using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP391.Contracts.Common;
using SWP391.Repositories.Interfaces;
using SWP391.Services.TicketServices;
using SWP391.WebAPI.Constants;

namespace SWP391.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Only admins can access testing endpoints
    public class BackgroundJobTestController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly OverdueTicketJob _overdueTicketJob;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public BackgroundJobTestController(
            IUnitOfWork unitOfWork,
            OverdueTicketJob overdueTicketJob,
            IBackgroundJobClient backgroundJobClient)
        {
            _unitOfWork = unitOfWork;
            _overdueTicketJob = overdueTicketJob;
            _backgroundJobClient = backgroundJobClient;
        }

        /// <summary>
        /// [TESTING ONLY] Subtract 1 year from ticket's resolve deadline to simulate overdue ticket
        /// </summary>
        /// <param name="ticketCode">The ticket code to manipulate</param>
        /// <response code="200">Deadline updated successfully.</response>
        /// <response code="400">Invalid ticket code.</response>
        /// <response code="404">Ticket not found.</response>
        [HttpPatch("subtract-deadline/{ticketCode}")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.BAD_REQUEST)]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.NOT_FOUND)]
        public async Task<IActionResult> SubtractTicketDeadline(string ticketCode)
        {
            if (string.IsNullOrWhiteSpace(ticketCode))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Ticket code cannot be empty"));
            }

            var ticket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Ticket '{ticketCode}' not found"));
            }

            if (!ticket.ResolveDeadline.HasValue)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    $"Ticket '{ticketCode}' does not have a resolve deadline"));
            }

            var originalDeadline = ticket.ResolveDeadline.Value;
            ticket.ResolveDeadline = originalDeadline.AddYears(-1);

            _unitOfWork.TicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return Ok(ApiResponse<object>.SuccessResponse(
                new
                {
                    TicketCode = ticketCode,
                    OriginalDeadline = originalDeadline,
                    NewDeadline = ticket.ResolveDeadline.Value,
                    Status = ticket.Status,
                    IsNowOverdue = ticket.ResolveDeadline.Value < DateTime.UtcNow
                },
                $"Ticket '{ticketCode}' deadline updated. Subtracted 1 year from deadline."));
        }

        /// <summary>
        /// [TESTING ONLY] Trigger the overdue ticket background job immediately
        /// </summary>
        /// <response code="200">Background job triggered successfully.</response>
        [HttpGet("trigger-overdue-job")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        public async Task<IActionResult> TriggerOverdueJob()
        {
            // Execute the job immediately and wait for completion
            await _overdueTicketJob.ProcessOverdueTicketsAsync();

            return Ok(ApiResponse<object>.SuccessResponse(
                new
                {
                    ExecutedAt = DateTime.UtcNow,
                    JobName = "ProcessOverdueTickets"
                },
                "Overdue ticket job executed successfully. Check logs for details."));
        }

        /// <summary>
        /// [TESTING ONLY] Enqueue overdue ticket job in Hangfire (runs in background)
        /// </summary>
        /// <response code="200">Background job enqueued successfully.</response>
        [HttpPost("enqueue-overdue-job")]
        [ProducesResponseType(typeof(ApiResponse<object>), ApiStatusCode.OK)]
        public IActionResult EnqueueOverdueJob()
        {
            var jobId = _backgroundJobClient.Enqueue<OverdueTicketJob>(
                job => job.ProcessOverdueTicketsAsync());

            return Ok(ApiResponse<object>.SuccessResponse(
                new
                {
                    JobId = jobId,
                    EnqueuedAt = DateTime.UtcNow,
                    JobName = "ProcessOverdueTickets",
                    DashboardUrl = "/hangfire"
                },
                "Overdue ticket job enqueued successfully. Monitor progress in Hangfire dashboard."));
        }
    }
}