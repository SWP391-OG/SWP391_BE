using Microsoft.Extensions.Logging;
using SWP391.Repositories.Interfaces;

namespace SWP391.Services.TicketServices
{
    /// <summary>
    /// Handles ticket business logic validation and duplicate detection.
    /// Separated from main services to promote reusability and testability.
    /// </summary>
    public class TicketValidationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TicketValidationService> _logger;

        public TicketValidationService(IUnitOfWork unitOfWork, ILogger<TicketValidationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Checks for duplicate tickets within 7-day window based on:
        /// - Same category
        /// - Same location (required)
        /// - Similar title (bidirectional match)
        /// - Status is NEW, ASSIGNED, or IN_PROGRESS (excludes RESOLVED, CANCELLED, CLOSED)
        /// </summary>
        public async Task<(bool HasDuplicates, List<string> DuplicateCodes)> CheckForDuplicatesAsync(
            int requesterId, string title, int categoryId, int locationId)
        {
            var createdAfter = DateTime.UtcNow.AddDays(-7);

            var duplicates = await _unitOfWork.TicketRepository.CheckForDuplicateTicketsAsync(
                requesterId, title, categoryId, locationId, createdAfter);

            var codes = duplicates.Select(t => t.TicketCode).ToList();
            return (duplicates.Any(), codes);
        }

        /// <summary>
        /// Validates if a status transition is allowed for staff operations.
        /// ✅ Staff can: ASSIGNED → IN_PROGRESS → RESOLVED
        /// ❌ Staff cannot: Cancel tickets (only Student/Admin can)
        /// </summary>
        public bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            var validTransitions = new Dictionary<string, string[]>
            {
                { "ASSIGNED", new[] { "IN_PROGRESS" } }, // Staff starts work
                { "IN_PROGRESS", new[] { "RESOLVED" } } // Staff completes work
            };

            return validTransitions.ContainsKey(currentStatus) &&
                   validTransitions[currentStatus].Contains(newStatus);
        }

        /// <summary>
        /// Gets the error message for an invalid status transition.
        /// </summary>
        public string GetStatusTransitionError(string currentStatus, string newStatus)
        {
            if (!IsValidStatusTransition(currentStatus, newStatus))
            {
                // Special message for CANCELLED attempts
                if (newStatus == "CANCELLED")
                    return "Staff cannot cancel tickets. Please contact an administrator if the ticket needs to be cancelled.";

                return $"Invalid status transition from {currentStatus} to {newStatus}. Allowed transitions: ASSIGNED → IN_PROGRESS → RESOLVED";
            }

            return string.Empty;
        }
    }
}