using AutoMapper;
using SWP391.Contracts.Common;
using SWP391.Contracts.Ticket;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;

namespace SWP391.Services.TicketServices
{
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TicketService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

            // Validate priority
            if (!new[] { "LOW", "MEDIUM", "HIGH" }.Contains(dto.Priority.ToUpper()))
                return (false, "Invalid priority. Must be LOW, MEDIUM, or HIGH", null);

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
                Priority = dto.Priority.ToUpper(),
                CreatedAt = DateTime.UtcNow,
                ResolveDeadline = CalculateResolveDeadline(category.SlaResolveHours ?? 24)
            };

            await _unitOfWork.TicketRepository.CreateAsync(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Reload with navigation properties
            var createdTicket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticket.TicketCode);
            var ticketDto = _mapper.Map<TicketDto>(createdTicket);

            return (true, "Ticket created successfully", ticketDto);
        }

        /// <summary>
        /// Get all tickets created by the student
        /// </summary>
        public async Task<List<TicketDto>> GetMyTicketsAsync(int requesterId)
        {
            var tickets = await _unitOfWork.TicketRepository.GetTicketsByRequesterIdAsync(requesterId);
            return _mapper.Map<List<TicketDto>>(tickets);
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

            // Validate priority if provided
            if (!string.IsNullOrEmpty(dto.Priority) && 
                !new[] { "LOW", "MEDIUM", "HIGH" }.Contains(dto.Priority.ToUpper()))
                return (false, "Invalid priority. Must be LOW, MEDIUM, or HIGH");

            // Update fields
            if (!string.IsNullOrEmpty(dto.Title))
                ticket.Title = dto.Title;
            
            if (!string.IsNullOrEmpty(dto.Description))
                ticket.Description = dto.Description;
            
            if (!string.IsNullOrEmpty(dto.ImageUrl))
                ticket.ImageUrl = dto.ImageUrl;
            
            if (!string.IsNullOrEmpty(dto.Priority))
                ticket.Priority = dto.Priority.ToUpper();

            _unitOfWork.TicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "Ticket updated successfully");
        }

        /// <summary>
        /// Student cancels their own NEW ticket (Soft Delete - Status = CANCELLED)
        /// </summary>
        public async Task<(bool Success, string Message)> CancelTicketAsync(string ticketCode, int userId)
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

            // Soft delete - set status to CANCELLED
            ticket.Status = "CANCELLED";
            ticket.ClosedAt = DateTime.UtcNow;

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
        /// Get student's tickets with pagination and filtering (TicketCode, Status, Priority)
        /// </summary>
        public async Task<PaginatedResponse<TicketDto>> GetMyTicketsWithPaginationAsync(int requesterId, PaginationRequestDto request)
        {
            var (items, totalCount) = await _unitOfWork.TicketRepository.GetTicketsByRequesterIdWithPaginationAsync(
                requesterId,
                request.PageNumber,
                request.PageSize,
                request.TicketCode,
                request.Status,
                request.Priority
            );

            var ticketDtos = _mapper.Map<List<TicketDto>>(items);
            return new PaginatedResponse<TicketDto>(ticketDtos, totalCount, request.PageNumber, request.PageSize);
        }

        #endregion

        #region Admin Operations

        /// <summary>
        /// Admin assigns ticket automatically (finds staff with least workload)
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

            // Assign ticket
            ticket.AssignedTo = staffUser.Id;
            ticket.ManagedBy = adminId;
            ticket.Status = "ASSIGNED";

            _unitOfWork.TicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, 
                $"Ticket automatically assigned to {selectedStaff.StaffName} (Current workload: {selectedStaff.ActiveTicketCount} tickets)", 
                selectedStaff.StaffCode);
        }

        /// <summary>
        /// Admin assigns ticket manually to a specific staff
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

            ticket.AssignedTo = staff.Id;
            ticket.ManagedBy = adminId;
            ticket.Status = "ASSIGNED";

            _unitOfWork.TicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, $"Ticket manually assigned to {staff.FullName}");
        }

        /// <summary>
        /// Admin cancels any ticket regardless of status (Soft Delete - Status = CANCELLED)
        /// </summary>
        public async Task<(bool Success, string Message)> AdminCancelTicketAsync(string ticketCode, int adminId)
        {
            var ticket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);
            
            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.Status == "CANCELLED")
                return (false, "Ticket is already cancelled");

            if (ticket.Status == "CLOSED")
                return (false, "Ticket is already closed and cannot be cancelled");

            // Admin can cancel tickets in any status (except CLOSED and CANCELLED)
            ticket.Status = "CANCELLED";
            ticket.ClosedAt = DateTime.UtcNow;
            
            // Record who cancelled it for audit trail
            if (ticket.ManagedBy == null)
            {
                ticket.ManagedBy = adminId;
            }

            _unitOfWork.TicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "Ticket cancelled successfully by administrator");
        }

        /// <summary>
        /// Get all tickets with pagination and filtering (Admin) - TicketCode, Status, Priority
        /// </summary>
        public async Task<PaginatedResponse<TicketDto>> GetAllTicketsWithPaginationAsync(TicketSearchRequestDto request)
        {
            var (items, totalCount) = await _unitOfWork.TicketRepository.GetAllTicketsWithPaginationAsync(
                request.PageNumber,
                request.PageSize,
                request.TicketCode,
                request.Status,
                request.Priority
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

            return (true, $"Ticket status updated to {newStatus}");
        }

        /// <summary>
        /// Get all tickets assigned to the staff
        /// </summary>
        public async Task<List<TicketDto>> GetMyAssignedTicketsAsync(int staffId)
        {
            var tickets = await _unitOfWork.TicketRepository.GetTicketsByAssignedToAsync(staffId);
            return _mapper.Map<List<TicketDto>>(tickets);
        }

        /// <summary>
        /// Get staff's assigned tickets with pagination and filtering (TicketCode, Status, Priority)
        /// </summary>
        public async Task<PaginatedResponse<TicketDto>> GetMyAssignedTicketsWithPaginationAsync(int staffId, PaginationRequestDto request)
        {
            var (items, totalCount) = await _unitOfWork.TicketRepository.GetTicketsByAssignedToWithPaginationAsync(
                staffId,
                request.PageNumber,
                request.PageSize,
                request.TicketCode,
                request.Status,
                request.Priority
            );

            var ticketDtos = _mapper.Map<List<TicketDto>>(items);
            return new PaginatedResponse<TicketDto>(ticketDtos, totalCount, request.PageNumber, request.PageSize);
        }

        #endregion

        #region Common Operations

        /// <summary>
        /// Get all tickets (Admin access)
        /// </summary>
        public async Task<List<TicketDto>> GetAllTicketsAsync()
        {
            var tickets = await _unitOfWork.TicketRepository.GetAllTicketsAsync();
            return _mapper.Map<List<TicketDto>>(tickets);
        }

        /// <summary>
        /// Get ticket by code
        /// </summary>
        public async Task<TicketDto> GetTicketByCodeAsync(string ticketCode)
        {
            var ticket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);
            return _mapper.Map<TicketDto>(ticket);
        }

        /// <summary>
        /// Get tickets by status
        /// </summary>
        public async Task<List<TicketDto>> GetTicketsByStatusAsync(string status)
        {
            var tickets = await _unitOfWork.TicketRepository.GetTicketsByStatusAsync(status);
            return _mapper.Map<List<TicketDto>>(tickets);
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

            // Check for tickets created in the last 7 days
            var createdAfter = DateTime.UtcNow.AddDays(-7);
            
            var duplicates = await _unitOfWork.TicketRepository.CheckForDuplicateTicketsAsync(
                requesterId, 
                dto.Title, 
                category.Id, 
                createdAfter);

            var duplicateDtos = _mapper.Map<List<TicketDto>>(duplicates);
            return (duplicates.Any(), duplicateDtos);
        }

        #endregion

        #region Escalation

        /// <summary>
        /// Escalate a ticket (Admin manually escalates or auto-escalate overdue tickets)
        /// </summary>
        public async Task<(bool Success, string Message)> EscalateTicketAsync(string ticketCode, int adminId)
        {
            var ticket = await _unitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);
            
            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.Status == "RESOLVED" || ticket.Status == "CLOSED" || ticket.Status == "CANCELLED")
                return (false, "Cannot escalate tickets that are already resolved, closed, or cancelled");

            // Change priority to HIGH if not already
            if (ticket.Priority != "HIGH")
                ticket.Priority = "HIGH";

            // Record who escalated it
            if (ticket.ManagedBy == null)
                ticket.ManagedBy = adminId;

            // Optional: You could add an "ESCALATED" status or keep current status
            // ticket.Status = "ESCALATED";

            _unitOfWork.TicketRepository.Update(ticket);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, $"Ticket {ticketCode} escalated to HIGH priority");
        }

        #endregion
    }
}