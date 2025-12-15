using SWP391.Repositories.Models;

namespace SWP391.Repositories.Interfaces
{
    public interface ITicketRepository
    {
        Task<Ticket?> GetTicketByCodeAsync(string ticketCode);
        
        // Pagination methods
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

        // Other methods
        Task<int> GetActiveTicketCountByStaffIdAsync(int staffId);
        Task<List<(string StaffCode, string StaffName, int ActiveTicketCount, string DepartmentCode)>> GetStaffWorkloadByDepartmentCodeAsync(string deptCode);
        Task<bool> HasUserProvidedFeedbackAsync(string ticketCode);
        
        // Overdue & Duplicate methods
        Task<List<Ticket>> GetOverdueTicketsAsync();
        Task<List<Ticket>> GetOverdueTicketsByStaffIdAsync(int staffId);
        
        // UPDATED: Added locationId parameter
        Task<List<Ticket>> CheckForDuplicateTicketsAsync(int requesterId, string title, int categoryId, int? locationId, DateTime createdAfter);
    }
}
