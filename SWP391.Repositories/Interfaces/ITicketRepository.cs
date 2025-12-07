using SWP391.Repositories.Models;

namespace SWP391.Repositories.Interfaces
{
    public interface ITicketRepository
    {
        Task<Ticket?> GetTicketByCodeAsync(string ticketCode);
        
        // Pagination methods - all use the same 3 filters (TicketCode, Status, Priority)
        Task<(List<Ticket> Items, int TotalCount)> GetAllTicketsWithPaginationAsync(
            int pageNumber, 
            int pageSize, 
            string? ticketCode,
            string? status, 
            string? priority);

        Task<(List<Ticket> Items, int TotalCount)> GetTicketsByRequesterIdWithPaginationAsync(
            int requesterId,
            int pageNumber,
            int pageSize,
            string? ticketCode,
            string? status,
            string? priority);

        Task<(List<Ticket> Items, int TotalCount)> GetTicketsByAssignedToWithPaginationAsync(
            int staffId,
            int pageNumber,
            int pageSize,
            string? ticketCode,
            string? status,
            string? priority);

        // Legacy methods (kept for backward compatibility)
        Task<List<Ticket>> GetAllTicketsAsync();
        Task<List<Ticket>> GetTicketsByRequesterIdAsync(int requesterId);
        Task<List<Ticket>> GetTicketsByAssignedToAsync(int staffId);
        Task<List<Ticket>> GetTicketsByStatusAsync(string status);
        
        // Other methods
        Task<int> GetActiveTicketCountByStaffIdAsync(int staffId);
        Task<List<(string StaffCode, string StaffName, int ActiveTicketCount, string DepartmentCode)>> GetStaffWorkloadByDepartmentCodeAsync(string deptCode);
        Task<bool> HasUserProvidedFeedbackAsync(string ticketCode);
        
        // New methods
        Task<List<Ticket>> GetOverdueTicketsAsync();
        Task<List<Ticket>> GetOverdueTicketsByStaffIdAsync(int staffId);
        Task<List<Ticket>> CheckForDuplicateTicketsAsync(int requesterId, string title, int categoryId, DateTime createdAfter);
    }
}
