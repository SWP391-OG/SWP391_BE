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
    /// 
    /// Business Rules:
    /// - Staff CANNOT cancel tickets (only Student/Admin can)
    /// - ASSIGNED → IN_PROGRESS: No notes required (just start working)
    /// - IN_PROGRESS → RESOLVED: Resolution notes REQUIRED (must explain what was done)
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
        /// Updates ticket status with business rule enforcement:
        /// - ASSIGNED → IN_PROGRESS: Staff starts working (no notes required)
        /// - IN_PROGRESS → RESOLVED: Staff completes work (resolution notes REQUIRED)
        /// - Staff CANNOT cancel tickets
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

            // Prevent idempotent updates
            if (ticket.Status == newStatus)
                return (false, $"Ticket is already in {newStatus} status");

            // Validate status transition (will reject CANCELLED attempts)
            if (!_validationService.IsValidStatusTransition(ticket.Status, newStatus))
            {
                var errorMessage = _validationService.GetStatusTransitionError(ticket.Status, newStatus);
                Logger.LogWarning(
                    "Invalid status transition attempted by staff {StaffId} for ticket {TicketCode}: {CurrentStatus} → {NewStatus}",
                    staffId, ticketCode, ticket.Status, newStatus);
                return (false, errorMessage);
            }

            // Business Rule: IN_PROGRESS → RESOLVED requires resolution notes
            if (ticket.Status == "IN_PROGRESS" && newStatus == "RESOLVED")
            {
                if (string.IsNullOrWhiteSpace(dto.ResolutionNotes))
                {
                    Logger.LogWarning(
                        "Staff {StaffId} attempted to resolve ticket {TicketCode} without providing resolution notes",
                        staffId, ticketCode);
                    return (false, "Resolution notes are required when marking a ticket as RESOLVED. Please explain what was done to resolve the issue.");
                }

                // Store resolution notes in the Note field
                ticket.Note = string.IsNullOrWhiteSpace(ticket.Note)
                    ? $"[RESOLVED BY STAFF] {dto.ResolutionNotes}"
                    : $"{ticket.Note}\n[RESOLVED BY STAFF] {dto.ResolutionNotes}";
                
                ticket.ResolvedAt = DateTime.UtcNow;
                
                Logger.LogInformation(
                    "Ticket {TicketCode} resolved by staff {StaffId}. Resolution: {ResolutionNotes}",
                    ticketCode, staffId, dto.ResolutionNotes);
            }
            else if (ticket.Status == "ASSIGNED" && newStatus == "IN_PROGRESS")
            {
                // No additional notes required when starting work
                Logger.LogInformation(
                    "Staff {StaffId} started working on ticket {TicketCode}",
                    staffId, ticketCode);
            }

            // Update status
            ticket.Status = newStatus;

            UnitOfWork.TicketRepository.Update(ticket);
            await UnitOfWork.SaveChangesWithTransactionAsync();

            // Notify student (non-blocking)
            try
            {
                var notificationMessage = newStatus == "RESOLVED"
                    ? $"Your ticket has been resolved. Resolution: {dto.ResolutionNotes}"
                    : $"Your ticket status has been updated to {newStatus}";

                await NotificationService.NotifyStudentOfTicketUpdateAsync(
                    ticket.RequesterId, ticketCode, notificationMessage);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex,
                    "Failed to notify student {StudentId} for ticket {TicketCode}. Status was updated successfully.",
                    ticket.RequesterId, ticketCode);
            }

            Logger.LogInformation(
                "Ticket {TicketCode} status updated to {NewStatus} by staff {StaffId}",
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