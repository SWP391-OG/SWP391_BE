using SWP391.Contracts.Common;
using SWP391.Contracts.Ticket;

namespace SWP391.Services.TicketServices
{
    public interface ITicketService
    {
        // Student operations
        Task<(bool Success, string Message, TicketDto Data)> CreateTicketAsync(CreateTicketRequestDto dto, int requesterId);
        Task<PaginatedResponse<TicketDto>> GetMyTicketsWithPaginationAsync(int requesterId, PaginationRequestDto request);
        Task<(bool Success, string Message)> UpdateTicketAsync(string ticketCode, UpdateTicketDto dto, int userId);
        Task<(bool Success, string Message)> ProvideFeedbackAsync(string ticketCode, TicketFeedbackDto dto, int requesterId);
        Task<(bool Success, string Message)> CancelTicketAsync(string ticketCode, int userId, string reason);

        // Admin operations
        Task<(bool Success, string Message, string AssignedStaffCode)> AssignTicketAutomaticallyAsync(string ticketCode, int adminId);
        Task<(bool Success, string Message)> AssignTicketManuallyAsync(string ticketCode, string staffCode, int adminId);
        Task<(bool Success, string Message)> AdminCancelTicketAsync(string ticketCode, int adminId, string reason);
        Task<List<StaffWorkloadDto>> GetStaffWorkloadByDepartmentCodeAsync(string deptCode);
        Task<PaginatedResponse<TicketDto>> GetAllTicketsWithPaginationAsync(TicketSearchRequestDto request);

        // Staff operations
        Task<(bool Success, string Message)> UpdateTicketStatusAsync(string ticketCode, UpdateTicketStatusDto dto, int staffId);
        Task<PaginatedResponse<TicketDto>> GetMyAssignedTicketsWithPaginationAsync(int staffId, PaginationRequestDto request);

        // Common operations
        Task<TicketDto> GetTicketByCodeAsync(string ticketCode);
    }
}