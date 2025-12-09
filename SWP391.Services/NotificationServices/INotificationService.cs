using SWP391.Contracts.Common;
using SWP391.Contracts.Notification;

namespace SWP391.Services.NotificationServices
{
    public interface INotificationService
    {
        // Get notifications for current user
        Task<PaginatedResponse<NotificationDto>> GetMyNotificationsAsync(int userId, NotificationPaginationRequestDto request);
        Task<int> GetUnreadCountAsync(int userId);

        // Mark notifications as read
        Task<(bool Success, string Message)> MarkAsReadAsync(int notificationId, int userId);
        Task<(bool Success, string Message)> MarkAllAsReadAsync(int userId);

        // Create notifications (used internally by ticket service)
        Task CreateNotificationAsync(int userId, string message, string type, string ticketCode);
        Task NotifyAdminsOfNewTicketAsync(string ticketCode, string ticketTitle);
        Task NotifyStaffOfAssignmentAsync(int staffId, string ticketCode, string ticketTitle);
        Task NotifyStudentOfTicketUpdateAsync(int studentId, string ticketCode, string message);
    }
}