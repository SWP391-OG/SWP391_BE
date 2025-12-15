using AutoMapper;
using Microsoft.Extensions.Logging;
using SWP391.Contracts.Common;
using SWP391.Contracts.Ticket;
using SWP391.Repositories.Interfaces;
using SWP391.Services.NotificationServices;
using SWP391.Services.TicketServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWP391.Services.TicketServices
{
    /// <summary>
    /// Handles all admin-specific ticket operations:
    /// - Automatic ticket assignment
    /// - Manual ticket assignment
    /// - Admin cancellation
    /// - Ticket escalation
    /// - Staff workload monitoring
    /// </summary>
    public class AdminTicketService : BaseTicketService
    {
        public AdminTicketService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            ILogger<AdminTicketService> logger)
            : base(unitOfWork, mapper, notificationService, logger)
        {
        }

        /// <summary>
        /// Automatically assigns ticket to staff with least workload in correct department.
        /// </summary>
        public async Task<(bool Success, string Message, string AssignedStaffCode)> AssignTicketAutomaticallyAsync(
            string ticketCode, int adminId)
        {
            var ticket = await UnitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found", string.Empty);

            if (ticket.Status != "NEW")
                return (false, "Only NEW tickets can be assigned", string.Empty);

            // Determine department
            var category = await UnitOfWork.CategoryRepository.GetByIdAsync(ticket.CategoryId);
            if (category == null)
                return (false, "Category not found", string.Empty);

            var department = await UnitOfWork.DepartmentRepository.GetByIdAsync(category.DepartmentId);
            if (department == null)
                return (false, "Department not found", string.Empty);

            // Get staff workload
            var staffWorkload = await UnitOfWork.TicketRepository
                .GetStaffWorkloadByDepartmentCodeAsync(department.DeptCode);

            if (!staffWorkload.Any())
            {
                Logger.LogWarning(
                    "Auto-assignment failed for ticket {TicketCode}: No active staff in {DepartmentName}",
                    ticketCode, department.DeptName);
                return (false,
                    $"No available staff in the {department.DeptName} department. Please assign manually.",
                    string.Empty);
            }

            // Select staff with least workload
            var selectedStaff = staffWorkload.OrderBy(s => s.ActiveTicketCount).First();
            var staffUser = await UnitOfWork.UserRepository.GetByUserCodeAsync(selectedStaff.StaffCode);
            if (staffUser == null)
                return (false, "Selected staff not found", string.Empty);

            // Assign ticket
            ticket.AssignedTo = staffUser.Id;
            ticket.ManagedBy = adminId;
            ticket.Status = "ASSIGNED";
            ticket.ContactPhone = staffUser.PhoneNumber;

            UnitOfWork.TicketRepository.Update(ticket);
            await UnitOfWork.SaveChangesWithTransactionAsync();

            // Notify staff (non-blocking)
            try
            {
                await NotificationService.NotifyStaffOfAssignmentAsync(staffUser.Id, ticket.TicketCode, ticket.Title);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to notify staff {StaffId} for ticket {TicketCode}",
                    staffUser.Id, ticket.TicketCode);
            }

            Logger.LogInformation(
                "Ticket {TicketCode} auto-assigned to {StaffName} by admin {AdminId}. Workload: {Workload}",
                ticketCode, selectedStaff.StaffName, adminId, selectedStaff.ActiveTicketCount);

            return (true,
                $"Ticket automatically assigned to {selectedStaff.StaffName} (Current workload: {selectedStaff.ActiveTicketCount} tickets)",
                selectedStaff.StaffCode);
        }

        /// <summary>
        /// Manually assigns ticket to a specific staff member.
        /// </summary>
        public async Task<(bool Success, string Message)> AssignTicketManuallyAsync(
            string ticketCode, string staffCode, int adminId)
        {
            var ticket = await UnitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.Status != "NEW")
                return (false, "Only NEW tickets can be assigned");

            // Determine required department
            var category = await UnitOfWork.CategoryRepository.GetByIdAsync(ticket.CategoryId);
            if (category == null)
                return (false, "Category not found");

            var department = await UnitOfWork.DepartmentRepository.GetByIdAsync(category.DepartmentId);
            if (department == null)
                return (false, "Department not found");

            // Validate staff
            var staff = await UnitOfWork.UserRepository.GetByUserCodeAsync(staffCode);
            if (staff == null)
                return (false, "Staff not found");

            if (staff.Status != "ACTIVE")
                return (false, $"Staff {staff.FullName} is not active. Current status: {staff.Status}");

            if (!staff.DepartmentId.HasValue)
                return (false, $"Staff {staff.FullName} is not assigned to any department.");

            if (staff.DepartmentId.Value != category.DepartmentId)
            {
                var staffDepartment = await UnitOfWork.DepartmentRepository.GetByIdAsync(staff.DepartmentId.Value);

                Logger.LogWarning(
                    "Manual assignment failed: Staff {StaffCode} belongs to {StaffDept} but ticket requires {RequiredDept}",
                    staffCode, staffDepartment?.DeptName, department.DeptName);

                return (false,
                    $"Staff {staff.FullName} belongs to {staffDepartment?.DeptName ?? "Unknown Department"} " +
                    $"but this ticket requires {department.DeptName} department.");
            }

            // Assign ticket
            ticket.AssignedTo = staff.Id;
            ticket.ManagedBy = adminId;
            ticket.Status = "ASSIGNED";
            ticket.ContactPhone = staff.PhoneNumber;

            UnitOfWork.TicketRepository.Update(ticket);
            await UnitOfWork.SaveChangesWithTransactionAsync();

            // Notify staff (non-blocking)
            try
            {
                await NotificationService.NotifyStaffOfAssignmentAsync(staff.Id, ticket.TicketCode, ticket.Title);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to notify staff {StaffId} for ticket {TicketCode}",
                    staff.Id, ticket.TicketCode);
            }

            Logger.LogInformation(
                "Ticket {TicketCode} manually assigned to {StaffName} from {DepartmentName} by admin {AdminId}",
                ticketCode, staff.FullName, department.DeptName, adminId);

            return (true, $"Ticket manually assigned to {staff.FullName} ({department.DeptName} department)");
        }

        /// <summary>
        /// Allows admin to cancel any ticket (except CLOSED/CANCELLED).
        /// </summary>
        public async Task<(bool Success, string Message)> AdminCancelTicketAsync(
            string ticketCode, int adminId, string reason)
        {
            var ticket = await UnitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.Status == "CANCELLED")
                return (false, "Ticket is already cancelled");

            if (ticket.Status == "CLOSED")
                return (false, "Ticket is already closed and cannot be cancelled");

            if (string.IsNullOrWhiteSpace(reason))
                return (false, "Cancellation reason is required");

            ticket.Status = "CANCELLED";
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.Note = $"[CANCELLED BY ADMIN] {reason}";

            if (ticket.ManagedBy == null)
                ticket.ManagedBy = adminId;

            UnitOfWork.TicketRepository.Update(ticket);
            await UnitOfWork.SaveChangesWithTransactionAsync();

            // Notify student (non-blocking)
            try
            {
                await NotificationService.NotifyStudentOfTicketUpdateAsync(
                    ticket.RequesterId, ticketCode,
                    $"Your ticket has been cancelled by administrator. Reason: {reason}");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to notify student {StudentId} for ticket {TicketCode}",
                    ticket.RequesterId, ticketCode);
            }

            Logger.LogInformation("Ticket {TicketCode} cancelled by admin {AdminId}. Reason: {Reason}",
                ticketCode, adminId, reason);

            return (true, "Ticket cancelled successfully by administrator");
        }

        /// <summary>
        /// Gets paginated list of all tickets (Admin view).
        /// </summary>
        public async Task<PaginatedResponse<TicketDto>> GetAllTicketsAsync(TicketSearchRequestDto request)
        {
            var (items, totalCount) = await UnitOfWork.TicketRepository.GetAllTicketsWithPaginationAsync(
                request.PageNumber, request.PageSize, request.TicketCode, request.Status, null);

            var ticketDtos = Mapper.Map<List<TicketDto>>(items);
            return new PaginatedResponse<TicketDto>(ticketDtos, totalCount, request.PageNumber, request.PageSize);
        }

        /// <summary>
        /// Gets staff workload for a department.
        /// </summary>
        public async Task<List<StaffWorkloadDto>> GetStaffWorkloadAsync(string deptCode)
        {
            var workload = await UnitOfWork.TicketRepository.GetStaffWorkloadByDepartmentCodeAsync(deptCode);

            return workload.Select(w => new StaffWorkloadDto
            {
                StaffCode = w.StaffCode,
                StaffName = w.StaffName,
                ActiveTicketCount = w.ActiveTicketCount,
                DepartmentCode = w.DepartmentCode
            }).OrderBy(w => w.ActiveTicketCount).ToList();
        }

        /// <summary>
        /// Escalates a ticket by recording which admin escalated it.
        /// </summary>
        public async Task<(bool Success, string Message)> EscalateTicketAsync(string ticketCode, int adminId)
        {
            var ticket = await UnitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.Status == "RESOLVED" || ticket.Status == "CLOSED" || ticket.Status == "CANCELLED")
                return (false, "Cannot escalate completed tickets");

            if (ticket.ManagedBy == null)
                ticket.ManagedBy = adminId;

            UnitOfWork.TicketRepository.Update(ticket);
            await UnitOfWork.SaveChangesWithTransactionAsync();

            Logger.LogInformation("Ticket {TicketCode} escalated by admin {AdminId}", ticketCode, adminId);

            return (true, $"Ticket {ticketCode} escalated successfully");
        }
    }
}