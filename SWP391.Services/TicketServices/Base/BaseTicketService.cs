using AutoMapper;
using Microsoft.Extensions.Logging;
using SWP391.Repositories.Interfaces;
using SWP391.Services.NotificationServices;

namespace SWP391.Services.TicketServices.Base
{
    /// <summary>
    /// Base class providing common dependencies for all ticket-related services.
    /// Reduces code duplication and ensures consistent dependency injection.
    /// </summary>
    public abstract class BaseTicketService
    {
        protected readonly IUnitOfWork UnitOfWork;
        protected readonly IMapper Mapper;
        protected readonly INotificationService NotificationService;
        protected readonly ILogger Logger;

        protected BaseTicketService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            ILogger logger)
        {
            UnitOfWork = unitOfWork;
            Mapper = mapper;
            NotificationService = notificationService;
            Logger = logger;
        }

        /// <summary>
        /// Generates a unique ticket code using timestamp and random number.
        /// Format: TKT{timestamp}{random} (e.g., TKT638501236789)
        /// </summary>
        protected string GenerateTicketCode()
        {
            var timestamp = DateTime.UtcNow.Ticks.ToString().Substring(8);
            var random = new Random().Next(1000, 9999);
            return $"TKT{timestamp}{random}";
        }

        /// <summary>
        /// Calculates ticket resolution deadline based on SLA hours.
        /// </summary>
        protected DateTime CalculateResolveDeadline(int slaHours)
        {
            return DateTime.UtcNow.AddHours(slaHours);
        }
    }
}