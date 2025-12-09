using SWP391.Repositories.Models;

namespace SWP391.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetNotificationsByUserIdAsync(int userId);
        Task<(List<Notification> Items, int TotalCount)> GetNotificationsByUserIdWithPaginationAsync(
            int userId,
            int pageNumber,
            int pageSize,
            bool? isRead);
        Task<int> GetUnreadCountByUserIdAsync(int userId);
        Task<Notification?> GetNotificationByIdAsync(int notificationId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
    }
}