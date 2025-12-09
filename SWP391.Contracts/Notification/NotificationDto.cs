namespace SWP391.Contracts.Notification
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string TicketCode { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class MarkNotificationAsReadDto
    {
        public int NotificationId { get; set; }
    }

    public class NotificationPaginationRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool? IsRead { get; set; } // Filter by read/unread
    }
}