using AutoMapper;
using SWP391.Contracts.Common;
using SWP391.Contracts.Ticket;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;
using SWP391.Services.NotificationServices;

namespace SWP391.Services.TicketServices
{
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public TicketService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        #region Student Operations
        /// <summary>
        /// Student creates a new ticket
        /// </summary>
        public async Task<(bool Success, string Message, TicketDto Data)> CreateTicketAsync(
            CreateTicketRequestDto dto, int requesterId)
        {
            // Validate category exists - USING CODE
            var category = await _unitOfWork.CategoryRepository.GetCategoryByCodeAsync(dto.CategoryCode);
            if (category == null)
                return (false, "Category not found", null);

            // Validate location exists - USING CODE
            var location = await _unitOfWork.LocationRepository.GetLocationByCodeAsync(dto.LocationCode);
            if (location == null)
                return (false, "Location not found", null);

            // Create ticket
            var ticket = new Ticket
            {
                TicketCode = GenerateTicketCode(),
                Title = dto.Title,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                RequesterId = requesterId,
                LocationId = location.Id,
                CategoryId = category.Id,
                Status = "NEW",
                ContactPhone = string.Empty, // Will be set when staff is assigned
                Note = string.Empty,
                CreatedAt = DateTime.UtcNow,
                ResolveDeadline = CalculateResolveDeadline(category.SlaResolveHours ?? 24)
            };

            await _unitOfWork.TicketRepository.CreateAsync(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Reload with navigation properties
            var createdTicket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticket.TicketCode);
            var ticketDto = _mapper.Map<TicketDto>(createdTicket);

            // Notify admins of new ticket
            await _notificationService.NotifyAdminsOfNewTicketAsync(ticket.TicketCode, ticket.Title);

            return (true, "Ticket created successfully", ticketDto);
        }

        /// <summary>
        /// Student updates their own NEW ticket
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateTicketAsync(
            string ticketCode, UpdateTicketDto dto, int userId)
        {
            var ticket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.RequesterId != userId)
                return (false, "You can only update your own tickets");

            if (ticket.Status != "NEW")
                return (false, "Only NEW tickets can be updated");

            // Update fields
            if (!string.IsNullOrEmpty(dto.Title))
                ticket.Title = dto.Title;

            if (!string.IsNullOrEmpty(dto.Description))
                ticket.Description = dto.Description;

            if (!string.IsNullOrEmpty(dto.ImageUrl))
                ticket.ImageUrl = dto.ImageUrl;

            _unitOfWork.TicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "Ticket updated successfully");
        }

        /// <summary>
        /// Student cancels their own NEW ticket (Soft Delete - Status = CANCELLED)
        /// NOW REQUIRES A REASON
        /// </summary>
        public async Task<(bool Success, string Message)> CancelTicketAsync(string ticketCode, int userId, string reason)
        {
            var ticket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.RequesterId != userId)
                return (false, "You can only cancel your own tickets");

            if (ticket.Status == "CANCELLED")
                return (false, "Ticket is already cancelled");

            if (ticket.Status == "CLOSED")
                return (false, "Ticket is already closed and cannot be cancelled");

            if (ticket.Status != "NEW")
                return (false, "Only NEW tickets can be cancelled by students. Please contact an administrator for assistance.");

            // Validate reason is provided
            if (string.IsNullOrWhiteSpace(reason))
                return (false, "Cancellation reason is required");

            // Soft delete - set status to CANCELLED and store the reason in Note
            ticket.Status = "CANCELLED";
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.Note = $"[CANCELLED BY STUDENT] {reason}";

            _unitOfWork.TicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "Ticket cancelled successfully");
        }

        /// <summary>
        /// Student provides feedback for a RESOLVED ticket
        /// </summary>
        public async Task<(bool Success, string Message)> ProvideFeedbackAsync(
            string ticketCode, TicketFeedbackDto dto, int requesterId)
        {
            var ticket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.RequesterId != requesterId)
                return (false, "You can only provide feedback on your own tickets");

            if (ticket.Status != "RESOLVED")
                return (false, "Ticket must be in RESOLVED status to provide feedback");

            if (ticket.RatingStars.HasValue)
                return (false, "Feedback has already been provided for this ticket");

            if (dto.RatingStars < 1 || dto.RatingStars > 5)
                return (false, "Rating stars must be between 1 and 5");

            ticket.RatingStars = dto.RatingStars;
            ticket.RatingComment = dto.RatingComment;
            ticket.Status = "CLOSED";
            ticket.ClosedAt = DateTime.UtcNow;

            _unitOfWork.TicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "Feedback submitted successfully and ticket is now closed");
        }

        /// <summary>
        /// Get student's tickets with pagination and filtering (TicketCode, Status)
        /// </summary>
        public async Task<PaginatedResponse<TicketDto>> GetMyTicketsWithPaginationAsync(int requesterId, PaginationRequestDto request)
        {
            var (items, totalCount) = await _unitOfWork.TicketRepository.GetTicketsByRequesterIdWithPaginationAsync(
                requesterId,
                request.PageNumber,
                request.PageSize,
                request.TicketCode,
                request.Status,
                null // No priority filter
            );

            var ticketDtos = _mapper.Map<List<TicketDto>>(items);
            return new PaginatedResponse<TicketDto>(ticketDtos, totalCount, request.PageNumber, request.PageSize);
        }

        #endregion

        #region Admin Operations

        /// <summary>
        /// Admin assigns ticket automatically (finds staff with least workload)
        /// SETS ContactPhone to assigned staff's phone number
        /// </summary>
        public async Task<(bool Success, string Message, string AssignedStaffCode)> AssignTicketAutomaticallyAsync(
            string ticketCode, int adminId)
        {
            var ticket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found", string.Empty);

            if (ticket.Status != "NEW")
                return (false, "Only NEW tickets can be assigned", string.Empty);

            // Get category to find department
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(ticket.CategoryId);
            if (category == null)
                return (false, "Category not found", string.Empty);

            var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(category.DepartmentId);
            if (department == null)
                return (false, "Department not found", string.Empty);

            // Get staff workload for the department
            var staffWorkload = await _unitOfWork.TicketRepository
                .GetStaffWorkloadByDepartmentCodeAsync(department.DeptCode);

            if (!staffWorkload.Any())
                return (false, "No available staff in the department", string.Empty);

            // Find staff with least workload
            var selectedStaff = staffWorkload.OrderBy(s => s.ActiveTicketCount).First();

            // Get staff user by code to get ID
            var staffUser = await _unitOfWork.UserRepository.GetByUserCodeAsync(selectedStaff.StaffCode);
            if (staffUser == null)
                return (false, "Selected staff not found", string.Empty);

            // Assign ticket and set ContactPhone
            ticket.AssignedTo = staffUser.Id;
            ticket.ManagedBy = adminId;
            ticket.Status = "ASSIGNED";
            ticket.ContactPhone = staffUser.PhoneNumber; // Set staff's phone number

            _unitOfWork.TicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Notify staff of assignment
            await _notificationService.NotifyStaffOfAssignmentAsync(staffUser.Id, ticket.TicketCode, ticket.Title);

            return (true,
                $"Ticket automatically assigned to {selectedStaff.StaffName} (Current workload: {selectedStaff.ActiveTicketCount} tickets)",
                selectedStaff.StaffCode);
        }

        /// <summary>
        /// Admin assigns ticket manually to a specific staff
        /// SETS ContactPhone to assigned staff's phone number
        /// </summary>
        public async Task<(bool Success, string Message)> AssignTicketManuallyAsync(
            string ticketCode, string staffCode, int adminId)
        {
            var ticket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.Status != "NEW")
                return (false, "Only NEW tickets can be assigned");

            // Validate staff exists and is active
            var staff = await _unitOfWork.UserRepository.GetByUserCodeAsync(staffCode);
            if (staff == null || staff.Status != "ACTIVE")
                return (false, "Staff not found or inactive");

            // Verify staff belongs to the same department as the category
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(ticket.CategoryId);
            if (staff.DepartmentId != category.DepartmentId)
                return (false, "Staff must belong to the same department as the ticket category");

            // Assign ticket and set ContactPhone
            ticket.AssignedTo = staff.Id;
            ticket.ManagedBy = adminId;
            ticket.Status = "ASSIGNED";
            ticket.ContactPhone = staff.PhoneNumber; // Set staff's phone number

            _unitOfWork.TicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Notify staff of assignment
            await _notificationService.NotifyStaffOfAssignmentAsync(staff.Id, ticket.TicketCode, ticket.Title);

            return (true, $"Ticket manually assigned to {staff.FullName}");
        }

        /// <summary>
        /// Admin cancels any ticket regardless of status (Soft Delete - Status = CANCELLED)
        /// NOW REQUIRES A REASON
        /// </summary>
        public async Task<(bool Success, string Message)> AdminCancelTicketAsync(string ticketCode, int adminId, string reason)
        {
            var ticket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.Status == "CANCELLED")
                return (false, "Ticket is already cancelled");

            if (ticket.Status == "CLOSED")
                return (false, "Ticket is already closed and cannot be cancelled");

            // Validate reason is provided
            if (string.IsNullOrWhiteSpace(reason))
                return (false, "Cancellation reason is required");

            // Admin can cancel tickets in any status (except CLOSED and CANCELLED)
            ticket.Status = "CANCELLED";
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.Note = $"[CANCELLED BY ADMIN] {reason}"; // Store reason in Note

            // Record who cancelled it for audit trail
            if (ticket.ManagedBy == null)
            {
                ticket.ManagedBy = adminId;
            }

            _unitOfWork.TicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Notify student of cancellation
            await _notificationService.NotifyStudentOfTicketUpdateAsync(
                ticket.RequesterId,
                ticketCode,
                $"Your ticket has been cancelled by administrator. Reason: {reason}");

            return (true, "Ticket cancelled successfully by administrator");
        }

        /// <summary>
        /// Get all tickets with pagination and filtering (Admin) - TicketCode, Status
        /// </summary>
        public async Task<PaginatedResponse<TicketDto>> GetAllTicketsWithPaginationAsync(TicketSearchRequestDto request)
        {
            var (items, totalCount) = await _unitOfWork.TicketRepository.GetAllTicketsWithPaginationAsync(
                request.PageNumber,
                request.PageSize,
                request.TicketCode,
                request.Status,
                null // No priority filter
            );

            var ticketDtos = _mapper.Map<List<TicketDto>>(items);
            return new PaginatedResponse<TicketDto>(ticketDtos, totalCount, request.PageNumber, request.PageSize);
        }

        /// <summary>
        /// Get staff workload for a department (for assignment purposes)
        /// </summary>
        public async Task<List<StaffWorkloadDto>> GetStaffWorkloadByDepartmentCodeAsync(string deptCode)
        {
            var workload = await _unitOfWork.TicketRepository.GetStaffWorkloadByDepartmentCodeAsync(deptCode);

            return workload.Select(w => new StaffWorkloadDto
            {
                StaffCode = w.StaffCode,
                StaffName = w.StaffName,
                ActiveTicketCount = w.ActiveTicketCount,
                DepartmentCode = w.DepartmentCode
            }).OrderBy(w => w.ActiveTicketCount).ToList();
        }

        #endregion

        #region Staff Operations

        /// <summary>
        /// Staff updates ticket status (ASSIGNED → IN_PROGRESS → RESOLVED)
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateTicketStatusAsync(
            string ticketCode, UpdateTicketStatusDto dto, int staffId)
        {
            var ticket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.AssignedTo != staffId)
                return (false, "You can only update tickets assigned to you");

            // Validate status transitions
            var validTransitions = new Dictionary<string, string[]>
            {
                { "ASSIGNED", new[] { "IN_PROGRESS", "CANCELLED" } },
                { "IN_PROGRESS", new[] { "RESOLVED", "CANCELLED" } }
            };

            var newStatus = dto.Status.ToUpper();

            if (!validTransitions.ContainsKey(ticket.Status))
                return (false, $"Tickets in {ticket.Status} status cannot be updated by staff");

            if (!validTransitions[ticket.Status].Contains(newStatus))
                return (false, $"Invalid status transition from {ticket.Status} to {newStatus}");

            // Update status
            ticket.Status = newStatus;

            // Set timestamps based on status
            if (newStatus == "RESOLVED")
            {
                ticket.ResolvedAt = DateTime.UtcNow;
            }
            else if (newStatus == "CANCELLED")
            {
                ticket.ClosedAt = DateTime.UtcNow;
            }

            _unitOfWork.TicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Notify student of status update
            await _notificationService.NotifyStudentOfTicketUpdateAsync(
                ticket.RequesterId,
                ticketCode,
                $"Your ticket status has been updated to {newStatus}");

            return (true, $"Ticket status updated to {newStatus}");
        }

        /// <summary>
        /// Get staff's assigned tickets with pagination and filtering (TicketCode, Status)
        /// </summary>
        public async Task<PaginatedResponse<TicketDto>> GetMyAssignedTicketsWithPaginationAsync(int staffId, PaginationRequestDto request)
        {
            var (items, totalCount) = await _unitOfWork.TicketRepository.GetTicketsByAssignedToWithPaginationAsync(
                staffId,
                request.PageNumber,
                request.PageSize,
                request.TicketCode,
                request.Status,
                null // No priority filter
            );

            var ticketDtos = _mapper.Map<List<TicketDto>>(items);
            return new PaginatedResponse<TicketDto>(ticketDtos, totalCount, request.PageNumber, request.PageSize);
        }

        #endregion

        #region Common Operations

        /// <summary>
        /// Get ticket by code
        /// </summary>
        public async Task<TicketDto> GetTicketByCodeAsync(string ticketCode)
        {
            var ticket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);
            return _mapper.Map<TicketDto>(ticket);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generate unique ticket code
        /// </summary>
        private string GenerateTicketCode()
        {
            return "TKT" + DateTime.UtcNow.Ticks.ToString().Substring(8);
        }

        /// <summary>
        /// Calculate resolve deadline based on SLA hours
        /// </summary>
        private DateTime CalculateResolveDeadline(int slaHours)
        {
            return DateTime.UtcNow.AddHours(slaHours);
        }

        #endregion

        #region Overdue & Duplicate Detection

        /// <summary>
        /// Get all overdue tickets (Admin only)
        /// </summary>
        public async Task<List<TicketDto>> GetOverdueTicketsAsync()
        {
            var tickets = await _unitOfWork.TicketRepository.GetOverdueTicketsAsync();
            return _mapper.Map<List<TicketDto>>(tickets);
        }

        /// <summary>
        /// Get overdue tickets for a specific staff member
        /// </summary>
        public async Task<List<TicketDto>> GetOverdueTicketsByStaffIdAsync(int staffId)
        {
            var tickets = await _unitOfWork.TicketRepository.GetOverdueTicketsByStaffIdAsync(staffId);
            return _mapper.Map<List<TicketDto>>(tickets);
        }

        /// <summary>
        /// Check for potential duplicate tickets before creating a new one
        /// </summary>
        public async Task<(bool HasDuplicates, List<TicketDto> PotentialDuplicates)> CheckForDuplicatesAsync(
            CreateTicketRequestDto dto, int requesterId)
        {
            var category = await _unitOfWork.CategoryRepository.GetCategoryByCodeAsync(dto.CategoryCode);
            if (category == null)
                return (false, new List<TicketDto>());

            // Resolve Location to check for facility duplicates
            var location = await _unitOfWork.LocationRepository.GetLocationByCodeAsync(dto.LocationCode);
            int? locationId = location?.Id;

            // Check for tickets created in the last 7 days
            var createdAfter = DateTime.UtcNow.AddDays(-7);

            var duplicates = await _unitOfWork.TicketRepository.CheckForDuplicateTicketsAsync(
                requesterId,
                dto.Title,
                category.Id,
                locationId,
                createdAfter);

            var duplicateDtos = _mapper.Map<List<TicketDto>>(duplicates);
            return (duplicates.Any(), duplicateDtos);
        }

        #endregion

        #region Escalation

        /// <summary>
        /// Escalate a ticket (Admin manually escalates or auto-escalate overdue tickets)
        /// REMOVED PRIORITY LOGIC
        /// </summary>
        public async Task<(bool Success, string Message)> EscalateTicketAsync(string ticketCode, int adminId)
        {
            var ticket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.Status == "RESOLVED" || ticket.Status == "CLOSED" || ticket.Status == "CANCELLED")
                return (false, "Cannot escalate tickets that are already resolved, closed, or cancelled");

            // Record who escalated it
            if (ticket.ManagedBy == null)
                ticket.ManagedBy = adminId;

            _unitOfWork.TicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, $"Ticket {ticketCode} escalated successfully");
        }

        #endregion
    }
}