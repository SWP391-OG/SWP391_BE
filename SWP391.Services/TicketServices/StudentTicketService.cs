using AutoMapper;
using Microsoft.Extensions.Logging;
using SWP391.Contracts.Common;
using SWP391.Contracts.Ticket;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;
using SWP391.Services.NotificationServices;
using SWP391.Services.TicketServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWP391.Services.TicketServices
{
    /// <summary>
    /// Handles all student-related ticket operations:
    /// - Create tickets with duplicate detection
    /// - Update NEW tickets
    /// - Cancel NEW tickets
    /// - Provide feedback on RESOLVED tickets
    /// - View own tickets
    /// </summary>
    public class StudentTicketService : BaseTicketService
    {
        private readonly TicketValidationService _validationService;

        public StudentTicketService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            ILogger<StudentTicketService> logger,
            TicketValidationService validationService)
            : base(unitOfWork, mapper, notificationService, logger)
        {
            _validationService = validationService;
        }

        /// <summary>
        /// Creates a new ticket with automatic duplicate detection.
        /// Validates category/location, checks for duplicates, then commits to database.
        /// </summary>
        public async Task<(bool Success, string Message, TicketDto Data)> CreateTicketAsync(
            CreateTicketRequestDto dto, int requesterId)
        {
            // Validate category exists
            var category = await UnitOfWork.CategoryRepository.GetCategoryByCodeAsync(dto.CategoryCode);
            if (category == null)
                return (false, "Category not found", null);

            // Validate location exists
            var location = await UnitOfWork.LocationRepository.GetLocationByCodeAsync(dto.LocationCode);
            if (location == null)
                return (false, "Location not found", null);

            // Check for duplicates
            var (hasDuplicates, duplicateCodes) = await _validationService.CheckForDuplicatesAsync(
                requesterId, dto.Title, category.Id, location.Id);

            if (hasDuplicates)
            {
                Logger.LogInformation(
                    "Duplicate ticket detection: User {RequesterId} attempted to create ticket similar to: {DuplicateCodes}",
                    requesterId, string.Join(", ", duplicateCodes));

                return (false,
                    $"Potential duplicate tickets found: {string.Join(", ", duplicateCodes)}. Please check existing tickets.",
                    null);
            }

            // Create ticket
            var ticket = new Ticket
            {
                TicketCode = GenerateTicketCode(),
                Title = dto.Title,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                RequesterId = requesterId,
                LocationId = location.Id,
                CategoryId = category.Id,
                Status = "NEW",
                ContactPhone = string.Empty,
                Note = string.Empty,
                CreatedAt = DateTime.UtcNow,
                ResolveDeadline = CalculateResolveDeadline(category.SlaResolveHours ?? 24)
            };

            await UnitOfWork.TicketRepository.CreateAsync(ticket);
            await UnitOfWork.SaveChangesWithTransactionAsync();

            // Reload with navigation properties
            var createdTicket = await UnitOfWork.TicketRepository.GetTicketByCodeAsync(ticket.TicketCode);
            var ticketDto = Mapper.Map<TicketDto>(createdTicket);

            // Send notification (non-blocking)
            try
            {
                await NotificationService.NotifyAdminsOfNewTicketAsync(ticket.TicketCode, ticket.Title);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to notify admins for ticket {TicketCode}", ticket.TicketCode);
            }

            Logger.LogInformation("Ticket {TicketCode} created by user {RequesterId}", ticket.TicketCode, requesterId);
            return (true, "Ticket created successfully", ticketDto);
        }

        /// <summary>
        /// Updates a NEW ticket (student can only update their own).
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateTicketAsync(
            string ticketCode, UpdateTicketDto dto, int userId)
        {
            var ticket = await UnitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.RequesterId != userId)
                return (false, "You can only update your own tickets");

            if (ticket.Status != "NEW")
                return (false, "Only NEW tickets can be updated");

            // Update fields
            if (!string.IsNullOrEmpty(dto.Title))
                ticket.Title = dto.Title;

            if (!string.IsNullOrEmpty(dto.Description))
                ticket.Description = dto.Description;

            if (!string.IsNullOrEmpty(dto.ImageUrl))
                ticket.ImageUrl = dto.ImageUrl;

            UnitOfWork.TicketRepository.Update(ticket);
            await UnitOfWork.SaveChangesWithTransactionAsync();

            Logger.LogInformation("Ticket {TicketCode} updated by user {UserId}", ticketCode, userId);
            return (true, "Ticket updated successfully");
        }

        /// <summary>
        /// Cancels a NEW ticket (soft delete).
        /// </summary>
        public async Task<(bool Success, string Message)> CancelTicketAsync(
            string ticketCode, int userId, string reason)
        {
            var ticket = await UnitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.RequesterId != userId)
                return (false, "You can only cancel your own tickets");

            if (ticket.Status == "CANCELLED")
                return (false, "Ticket is already cancelled");

            if (ticket.Status == "CLOSED")
                return (false, "Ticket is already closed and cannot be cancelled");

            if (ticket.Status != "NEW")
                return (false, "Only NEW tickets can be cancelled by students.");

            if (string.IsNullOrWhiteSpace(reason))
                return (false, "Cancellation reason is required");

            ticket.Status = "CANCELLED";
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.Note = $"[CANCELLED BY STUDENT] {reason}";

            UnitOfWork.TicketRepository.Update(ticket);
            await UnitOfWork.SaveChangesWithTransactionAsync();

            Logger.LogInformation("Ticket {TicketCode} cancelled by student {UserId}. Reason: {Reason}",
                ticketCode, userId, reason);
            return (true, "Ticket cancelled successfully");
        }

        /// <summary>
        /// Provides feedback for a RESOLVED ticket.
        /// </summary>
        public async Task<(bool Success, string Message)> ProvideFeedbackAsync(
            string ticketCode, TicketFeedbackDto dto, int requesterId)
        {
            var ticket = await UnitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);

            if (ticket == null)
                return (false, "Ticket not found");

            if (ticket.RequesterId != requesterId)
                return (false, "You can only provide feedback on your own tickets");

            if (ticket.Status != "RESOLVED")
                return (false, "Ticket must be in RESOLVED status to provide feedback");

            if (ticket.RatingStars.HasValue)
                return (false, "Feedback has already been provided");

            if (dto.RatingStars < 1 || dto.RatingStars > 5)
                return (false, "Rating stars must be between 1 and 5");

            ticket.RatingStars = dto.RatingStars;
            ticket.RatingComment = dto.RatingComment;
            ticket.Status = "CLOSED";
            ticket.ClosedAt = DateTime.UtcNow;

            UnitOfWork.TicketRepository.Update(ticket);
            await UnitOfWork.SaveChangesWithTransactionAsync();

            Logger.LogInformation("Ticket {TicketCode} closed with {Stars} stars by user {RequesterId}",
                ticketCode, dto.RatingStars, requesterId);
            return (true, "Feedback submitted successfully");
        }

        /// <summary>
        /// Gets paginated tickets for a student.
        /// </summary>
        public async Task<PaginatedResponse<TicketDto>> GetMyTicketsAsync(
            int requesterId, PaginationRequestDto request)
        {
            var (items, totalCount) = await UnitOfWork.TicketRepository
                .GetTicketsByRequesterIdWithPaginationAsync(
                    requesterId, request.PageNumber, request.PageSize,
                    request.TicketCode, request.Status, null);

            var ticketDtos = Mapper.Map<List<TicketDto>>(items);
            return new PaginatedResponse<TicketDto>(ticketDtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}