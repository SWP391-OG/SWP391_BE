using AutoMapper;
using SWP391.Contracts.Common;
using SWP391.Contracts.Notification;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;

namespace SWP391.Services.NotificationServices
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaginatedResponse<NotificationDto>> GetMyNotificationsAsync(
            int userId,
            NotificationPaginationRequestDto request)
        {
            var (items, totalCount) = await _unitOfWork.NotificationRepository
                .GetNotificationsByUserIdWithPaginationAsync(
                    userId,
                    request.PageNumber,
                    request.PageSize,
                    request.IsRead);

            var notificationDtos = _mapper.Map<List<NotificationDto>>(items);
            return new PaginatedResponse<NotificationDto>(
                notificationDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _unitOfWork.NotificationRepository.GetUnreadCountByUserIdAsync(userId);
        }

        public async Task<(bool Success, string Message)> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _unitOfWork.NotificationRepository.GetNotificationByIdAsync(notificationId);

            if (notification == null)
                return (false, "Notification not found");

            if (notification.UserId != userId)
                return (false, "You can only mark your own notifications as read");

            if (notification.IsRead)
                return (false, "Notification is already marked as read");

            await _unitOfWork.NotificationRepository.MarkAsReadAsync(notificationId);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "Notification marked as read");
        }

        public async Task<(bool Success, string Message)> MarkAllAsReadAsync(int userId)
        {
            await _unitOfWork.NotificationRepository.MarkAllAsReadAsync(userId);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "All notifications marked as read");
        }

        public async Task CreateNotificationAsync(int userId, string message, string type, string ticketCode)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                TicketCode = ticketCode,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.NotificationRepository.CreateAsync(notification);
            await _unitOfWork.SaveChangesWithTransactionAsync();
        }

        public async Task NotifyAdminsOfNewTicketAsync(string ticketCode, string ticketTitle)
        {
            // Get all admin users
            var adminRole = await _unitOfWork.RoleRepository.GetRoleByNameAsync("Admin");
            if (adminRole == null) return;

            var allUsers = await _unitOfWork.UserRepository.GetAllAsync();
            var admins = allUsers.Where(u => u.RoleId == adminRole.Id && u.Status == "ACTIVE").ToList();

            foreach (var admin in admins)
            {
                await CreateNotificationAsync(
                    admin.Id,
                    $"New ticket created: {ticketTitle}",
                    "TICKET_CREATED",
                    ticketCode);
            }
        }

        public async Task NotifyStaffOfAssignmentAsync(int staffId, string ticketCode, string ticketTitle)
        {
            await CreateNotificationAsync(
                staffId,
                $"You have been assigned to ticket: {ticketTitle}",
                "TICKET_ASSIGNED",
                ticketCode);
        }

        public async Task NotifyStudentOfTicketUpdateAsync(int studentId, string ticketCode, string message)
        {
            await CreateNotificationAsync(
                studentId,
                message,
                "TICKET_UPDATED",
                ticketCode);
        }
    }
}