using AutoMapper;
using Microsoft.Extensions.Logging;
using SWP391.Contracts.Common;
using SWP391.Contracts.Ticket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWP391.Services.TicketServices
{
    /// <summary>
    /// Facade service that orchestrates ticket operations across specialized services.
    /// Implements ITicketService by delegating to domain-specific services.
    /// </summary>
    public class TicketService : ITicketService
    {
        private readonly StudentTicketService _studentService;
        private readonly AdminTicketService _adminService;
        private readonly StaffTicketService _staffService;
        private readonly TicketQueryService _queryService;

        public TicketService(
            StudentTicketService studentService,
            AdminTicketService adminService,
            StaffTicketService staffService,
            TicketQueryService queryService)
        {
            _studentService = studentService;
            _adminService = adminService;
            _staffService = staffService;
            _queryService = queryService;
        }

        #region Student Operations - Delegated to StudentTicketService

        public Task<(bool Success, string Message, TicketDto Data)> CreateTicketAsync(
            CreateTicketRequestDto dto, int requesterId)
            => _studentService.CreateTicketAsync(dto, requesterId);

        public Task<(bool Success, string Message)> UpdateTicketAsync(
            string ticketCode, UpdateTicketDto dto, int userId)
            => _studentService.UpdateTicketAsync(ticketCode, dto, userId);

        public Task<(bool Success, string Message)> CancelTicketAsync(
            string ticketCode, int userId, string reason)
            => _studentService.CancelTicketAsync(ticketCode, userId, reason);

        public Task<(bool Success, string Message)> ProvideFeedbackAsync(
            string ticketCode, TicketFeedbackDto dto, int requesterId)
            => _studentService.ProvideFeedbackAsync(ticketCode, dto, requesterId);

        public Task<PaginatedResponse<TicketDto>> GetMyTicketsWithPaginationAsync(
            int requesterId, PaginationRequestDto request)
            => _studentService.GetMyTicketsAsync(requesterId, request);

        #endregion

        #region Admin Operations - Delegated to AdminTicketService

        public Task<(bool Success, string Message, string AssignedStaffCode)> AssignTicketAutomaticallyAsync(
            string ticketCode, int adminId)
            => _adminService.AssignTicketAutomaticallyAsync(ticketCode, adminId);

        public Task<(bool Success, string Message)> AssignTicketManuallyAsync(
            string ticketCode, string staffCode, int adminId)
            => _adminService.AssignTicketManuallyAsync(ticketCode, staffCode, adminId);

        public Task<(bool Success, string Message)> AdminCancelTicketAsync(
            string ticketCode, int adminId, string reason)
            => _adminService.AdminCancelTicketAsync(ticketCode, adminId, reason);

        public Task<PaginatedResponse<TicketDto>> GetAllTicketsWithPaginationAsync(
            TicketSearchRequestDto request)
            => _adminService.GetAllTicketsAsync(request);

        public Task<List<StaffWorkloadDto>> GetStaffWorkloadByDepartmentCodeAsync(string deptCode)
            => _adminService.GetStaffWorkloadAsync(deptCode);

        public Task<(bool Success, string Message)> EscalateTicketAsync(string ticketCode, int adminId)
            => _adminService.EscalateTicketAsync(ticketCode, adminId);

        #endregion

        #region Staff Operations - Delegated to StaffTicketService

        public Task<(bool Success, string Message)> UpdateTicketStatusAsync(
            string ticketCode, UpdateTicketStatusDto dto, int staffId)
            => _staffService.UpdateTicketStatusAsync(ticketCode, dto, staffId);

        public Task<PaginatedResponse<TicketDto>> GetMyAssignedTicketsWithPaginationAsync(
            int staffId, PaginationRequestDto request)
            => _staffService.GetMyAssignedTicketsAsync(staffId, request);

        public Task<List<TicketDto>> GetOverdueTicketsByStaffIdAsync(int staffId)
            => _staffService.GetMyOverdueTicketsAsync(staffId);

        #endregion

        #region Common Operations - Delegated to TicketQueryService

        public Task<TicketDto> GetTicketByCodeAsync(string ticketCode)
            => _queryService.GetTicketByCodeAsync(ticketCode);

        public Task<List<TicketDto>> GetOverdueTicketsAsync()
            => _queryService.GetOverdueTicketsAsync();

        public Task<(bool HasDuplicates, List<TicketDto> PotentialDuplicates)> CheckForDuplicatesAsync(
            CreateTicketRequestDto dto, int requesterId)
            => _queryService.CheckForDuplicatesAsync(dto, requesterId);

        #endregion
    }
}