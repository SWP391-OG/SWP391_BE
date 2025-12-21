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
                // Process NEW, ASSIGNED, and IN_PROGRESS tickets that are overdue
                if (ticket.Status != "NEW" && ticket.Status != "ASSIGNED" && ticket.Status != "IN_PROGRESS")
                    continue;

                ticket.Status = OverdueStatus;
                ticket.ClosedAt = now;

                // Add context based on original status
                var statusContext = ticket.Status switch
                {
                    "NEW" => "Ticket was never assigned to staff.",
                    "ASSIGNED" => "Staff did not start working on the ticket.",
                    "IN_PROGRESS" => "Staff did not complete the ticket in time.",
                    _ => ""
                };

                ticket.Note = string.IsNullOrWhiteSpace(ticket.Note)
                    ? $"{NotePrefix} {reason} {statusContext}"
                    : $"{ticket.Note}\n{NotePrefix} {reason} {statusContext}";

                _unitOfWork.TicketRepository.Update(ticket);
                updatedCount++;

                _logger.LogWarning(
                    "Ticket {TicketCode} marked as OVERDUE. Original status: {OriginalStatus}, Deadline: {Deadline}",
                    ticket.TicketCode, ticket.Status, ticket.ResolveDeadline);
            }

            await _unitOfWork.SaveChangesWithTransactionAsync();

            _logger.LogInformation("OverdueTicketJob: Marked {Count} tickets as {Status}.", updatedCount, OverdueStatus);
        }
    }
}