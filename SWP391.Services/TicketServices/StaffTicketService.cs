using AutoMapper;
using Microsoft.Extensions.Logging;
using SWP391.Contracts.Common;
using SWP391.Contracts.Ticket;
using SWP391.Repositories.Interfaces;
using SWP391.Services.NotificationServices;
using SWP391.Services.TicketServices.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWP391.Services.TicketServices
{
    /// <summary>
    /// Handles all staff-specific ticket operations:
    /// - Update ticket status (ASSIGNED → IN_PROGRESS → RESOLVED)
    /// - View assigned tickets
    /// - View overdue tickets
    /// </summary>
    public class StaffTicketService : BaseTicketService
    {
        private readonly TicketValidationService _validationService;

        public StaffTicketService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            ILogger<StaffTicketService> logger,
            TicketValidationService validationService)
            : base(unitOfWork, mapper, notificationService, logger)
        {
            _validationService = validationService;
        }

        /// <summary>
        /// Updates ticket status with validation for allowed transitions.
        /// Valid transitions: ASSIGNED → IN_PROGRESS → RESOLVED or CANCELLED
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateTicketStatusAsync(
            string ticketCode, UpdateTicketStatusDto dto, int staffId)
        {
            var ticket = await UnitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found");

            // Authorization: Staff can only update their assigned tickets
            if (ticket.AssignedTo != staffId)
                return (false, "You can only update tickets assigned to you");

            var newStatus = dto.Status.ToUpper();

            // Validate status transition
            if (!_validationService.IsValidStatusTransition(ticket.Status, newStatus))
            {
                var errorMessage = _validationService.GetStatusTransitionError(ticket.Status, newStatus);
                return (false, errorMessage);
            }

            // Update status
            ticket.Status = newStatus;

            // Set appropriate timestamps
            if (newStatus == "RESOLVED")
            {
                ticket.ResolvedAt = DateTime.UtcNow;
            }
            else if (newStatus == "CANCELLED")
            {
                ticket.ClosedAt = DateTime.UtcNow;
            }

            UnitOfWork.TicketRepository.Update(ticket);
            await UnitOfWork.SaveChangesWithTransactionAsync();

            // Notify student (non-blocking)
            try
            {
                await NotificationService.NotifyStudentOfTicketUpdateAsync(
                    ticket.RequesterId, ticketCode,
                    $"Your ticket status has been updated to {newStatus}");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex,
                    "Failed to notify student {StudentId} for ticket {TicketCode}. Status was updated successfully.",
                    ticket.RequesterId, ticketCode);
            }

            Logger.LogInformation("Ticket {TicketCode} status updated to {NewStatus} by staff {StaffId}",
                ticketCode, newStatus, staffId);

            return (true, $"Ticket status updated to {newStatus}");
        }

        /// <summary>
        /// Gets paginated tickets assigned to a staff member.
        /// </summary>
        public async Task<PaginatedResponse<TicketDto>> GetMyAssignedTicketsAsync(
            int staffId, PaginationRequestDto request)
        {
            var (items, totalCount) = await UnitOfWork.TicketRepository
                .GetTicketsByAssignedToWithPaginationAsync(
                    staffId, request.PageNumber, request.PageSize,
                    request.TicketCode, request.Status, null);

            var ticketDtos = Mapper.Map<List<TicketDto>>(items);
            return new PaginatedResponse<TicketDto>(ticketDtos, totalCount, request.PageNumber, request.PageSize);
        }

        /// <summary>
        /// Gets overdue tickets assigned to a specific staff member.
        /// </summary>
        public async Task<List<TicketDto>> GetMyOverdueTicketsAsync(int staffId)
        {
            var tickets = await UnitOfWork.TicketRepository.GetOverdueTicketsByStaffIdAsync(staffId);
            return Mapper.Map<List<TicketDto>>(tickets);
        }
    }
}