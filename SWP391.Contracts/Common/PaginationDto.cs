namespace SWP391.Contracts.Common
{
    /// <summary>
    /// Generic pagination request parameters
    /// </summary>
    public class PaginationRequestDto
    {
        private int _pageNumber = 1;
        private int _pageSize = 10;

        /// <summary>
        /// Page number (starts from 1, default: 1)
        /// </summary>
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        /// <summary>
        /// Number of items per page (default: 10, max: 100)
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : value > 100 ? 100 : value;
        }

        /// <summary>
        /// Filter by ticket code (partial match)
        /// </summary>
        public string? TicketCode { get; set; }

        /// <summary>
        /// Filter by status (NEW, ASSIGNED, IN_PROGRESS, RESOLVED, CLOSED, CANCELLED)
        /// </summary>
        public string? Status { get; set; }
    }

    /// <summary>
    /// Generic paginated response wrapper
    /// </summary>
    public class PaginatedResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
        public List<T> Items { get; set; } = new List<T>();

        public PaginatedResponse()
        {
        }

        public PaginatedResponse(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            HasPrevious = pageNumber > 1;
            HasNext = pageNumber < TotalPages;
            Items = items;
        }
    }

    /// <summary>
    /// Ticket-specific search (Admin) - inherits the same 2 filters
    /// </summary>
    public class TicketSearchRequestDto : PaginationRequestDto
    {
        // Inherits: TicketCode, Status from PaginationRequestDto
        // No additional filters needed - all roles use the same 2 fields
    }
}