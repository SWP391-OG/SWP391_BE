using Microsoft.Extensions.Logging;
using SWP391.Repositories.Interfaces;

namespace SWP391.Services.TicketServices
{
    public class OverdueTicketJob
    {
        private const string OverdueStatus = "OVERDUE";
        private const string NotePrefix = "[CANCELLED BY SYSTEM]";

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OverdueTicketJob> _logger;

        public OverdueTicketJob(IUnitOfWork unitOfWork, ILogger<OverdueTicketJob> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ProcessOverdueTicketsAsync()
        {
            var overdueTickets = await _unitOfWork.TicketRepository.GetOverdueTicketsAsync();

            if (overdueTickets.Count == 0)
            {
                _logger.LogInformation("OverdueTicketJob: No overdue tickets found.");
                return;
            }

            var now = DateTime.UtcNow;
            var reason = $"Ticket exceeded SLA deadline at {now:O}.";

            var updatedCount = 0;

            foreach (var ticket in overdueTickets)
            {
                // Safety: only operate on ASSIGNED/IN_PROGRESS (repository already filters this)
                if (ticket.Status != "ASSIGNED" && ticket.Status != "IN_PROGRESS")
                    continue;

                ticket.Status = OverdueStatus;
                ticket.ClosedAt = now;

                ticket.Note = string.IsNullOrWhiteSpace(ticket.Note)
                    ? $"{NotePrefix} {reason}"
                    : $"{ticket.Note}\n{NotePrefix} {reason}";

                _unitOfWork.TicketRepository.Update(ticket);
                updatedCount++;
            }

            await _unitOfWork.SaveChangesWithTransactionAsync();

            _logger.LogInformation("OverdueTicketJob: Marked {Count} tickets as {Status}.", updatedCount, OverdueStatus);
        }
    }
}