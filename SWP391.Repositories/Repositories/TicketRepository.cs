using Microsoft.EntityFrameworkCore;
using SWP391.Repositories.Basic;
using SWP391.Repositories.DBContext;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;

namespace SWP391.Repositories.Repositories
{
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        public TicketRepository() => _context ??= new FPTechnicalContext();

        public TicketRepository(FPTechnicalContext context) => _context = context;

        public async Task<Ticket?> GetTicketByCodeAsync(string ticketCode)
        {
            return await _context.Tickets
                .Include(t => t.Requester)
                .Include(t => t.AssignedToNavigation)
                .Include(t => t.ManagedByNavigation)
                .Include(t => t.Location)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);
        }

        #region Pagination Methods

        /// <summary>
        /// Get all tickets with pagination and filtering (Admin)
        /// </summary>
        public async Task<(List<Ticket> Items, int TotalCount)> GetAllTicketsWithPaginationAsync(
            int pageNumber,
            int pageSize,
            string? ticketCode,
            string? status,
            string? priority)
        {
            var query = _context.Tickets
                .Include(t => t.Requester)
                .Include(t => t.AssignedToNavigation)
                .Include(t => t.ManagedByNavigation)
                .Include(t => t.Location)
                .Include(t => t.Category)
                .AsQueryable();

            query = ApplyFilters(query, ticketCode, status, priority);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Get student's tickets with pagination (Student view)
        /// </summary>
        public async Task<(List<Ticket> Items, int TotalCount)> GetTicketsByRequesterIdWithPaginationAsync(
            int requesterId,
            int pageNumber,
            int pageSize,
            string? ticketCode,
            string? status,
            string? priority)
        {
            var query = _context.Tickets
                .Include(t => t.Requester)
                .Include(t => t.AssignedToNavigation)
                .Include(t => t.Location)
                .Include(t => t.Category)
                .Where(t => t.RequesterId == requesterId)
                .AsQueryable();

            query = ApplyFilters(query, ticketCode, status, priority);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        /// <summary>
        /// Get staff's assigned tickets with pagination (Staff view)
        /// </summary>
        public async Task<(List<Ticket> Items, int TotalCount)> GetTicketsByAssignedToWithPaginationAsync(
            int staffId,
            int pageNumber,
            int pageSize,
            string? ticketCode,
            string? status,
            string? priority)
        {
            var query = _context.Tickets
                .Include(t => t.Requester)
                .Include(t => t.AssignedToNavigation)
                .Include(t => t.ManagedByNavigation)
                .Include(t => t.Location)
                .Include(t => t.Category)
                .Where(t => t.AssignedTo == staffId)
                .AsQueryable();

            query = ApplyFilters(query, ticketCode, status, priority);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        #endregion

        #region Other Methods

        public async Task<int> GetActiveTicketCountByStaffIdAsync(int staffId)
        {
            return await _context.Tickets
                .Where(t => t.AssignedTo == staffId &&
                           (t.Status == "ASSIGNED" || t.Status == "IN_PROGRESS"))
                .CountAsync();
        }

        // Get staff workload by department code
        public async Task<List<(string StaffCode, string StaffName, int ActiveTicketCount, string DepartmentCode)>>
            GetStaffWorkloadByDepartmentCodeAsync(string deptCode)
        {
            var staffWorkload = await _context.Users
                .Where(u => u.Department.DeptCode == deptCode &&
                           u.Status == "ACTIVE" &&
                           u.Role.RoleName == "Staff")
                .Select(u => new
                {
                    StaffCode = u.UserCode,
                    StaffName = u.FullName,
                    ActiveTicketCount = u.TicketAssignedToNavigations
                        .Count(t => t.Status == "ASSIGNED" || t.Status == "IN_PROGRESS"),
                    DepartmentCode = u.Department.DeptCode
                })
                .ToListAsync();

            return staffWorkload
                .Select(s => (s.StaffCode, s.StaffName, s.ActiveTicketCount, s.DepartmentCode))
                .ToList();
        }

        public async Task<bool> HasUserProvidedFeedbackAsync(string ticketCode)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);

            return ticket?.RatingStars.HasValue ?? false;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Apply filters: TicketCode, Status, Priority (same for all roles)
        /// </summary>
        private IQueryable<Ticket> ApplyFilters(
            IQueryable<Ticket> query,
            string? ticketCode,
            string? status,
            string? priority)
        {
            // Ticket Code filter (partial match)
            if (!string.IsNullOrWhiteSpace(ticketCode))
            {
                query = query.Where(t => t.TicketCode.ToLower().Contains(ticketCode.ToLower()));
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status.ToUpper());
            }

            return query;
        }

        #endregion

        #region Overdue & Duplicate Detection

        /// <summary>
        /// Get all overdue tickets (ResolveDeadline passed and status is ASSIGNED or IN_PROGRESS)
        /// </summary>
        public async Task<List<Ticket>> GetOverdueTicketsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Tickets
                .Include(t => t.Requester)
                .Include(t => t.AssignedToNavigation)
                .Include(t => t.ManagedByNavigation)
                .Include(t => t.Location)
                .Include(t => t.Category)
                .Where(t => t.ResolveDeadline.HasValue &&
                            t.ResolveDeadline.Value < now &&
                            (t.Status == "ASSIGNED" || t.Status == "IN_PROGRESS"))
                .OrderBy(t => t.ResolveDeadline)
                .ToListAsync();
        }

        /// <summary>
        /// Get overdue tickets assigned to a specific staff member
        /// </summary>
        public async Task<List<Ticket>> GetOverdueTicketsByStaffIdAsync(int staffId)
        {
            var now = DateTime.UtcNow;
            return await _context.Tickets
                .Include(t => t.Requester)
                .Include(t => t.Location)
                .Include(t => t.Category)
                .Where(t => t.AssignedTo == staffId &&
                            t.ResolveDeadline.HasValue &&
                            t.ResolveDeadline.Value < now &&
                            (t.Status == "ASSIGNED" || t.Status == "IN_PROGRESS"))
                .OrderBy(t => t.ResolveDeadline)
                .ToListAsync();
        }

        /// <summary>
        /// Check for potential duplicate tickets (same category AND same location, similar title, created recently)
        /// Only checks active/in-progress tickets: NEW, ASSIGNED, IN_PROGRESS
        /// </summary>
        public async Task<List<Ticket>> CheckForDuplicateTicketsAsync(
            int requesterId,
            string title,
            int categoryId,
            int? locationId,
            DateTime createdAfter)
        {
            var searchTitle = title.ToLower().Trim();

            // Only check tickets that are truly "active" (not yet resolved)
            var activeStatuses = new[] { "NEW", "ASSIGNED", "IN_PROGRESS" };

            return await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Location)
                .Where(t => t.CategoryId == categoryId &&
                            t.CreatedAt >= createdAfter &&
                            activeStatuses.Contains(t.Status) &&
                            // Both same category AND same location required
                            locationId.HasValue && t.LocationId == locationId.Value &&
                            // Bidirectional title check: "Wifi broken" matches "Wifi" and vice versa
                            (t.Title.ToLower().Contains(searchTitle) || searchTitle.Contains(t.Title.ToLower())))
                .ToListAsync();
        }

        #endregion
    }
}