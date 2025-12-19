using AutoMapper;
using Microsoft.Extensions.Logging;
using SWP391.Contracts.Ticket;
using SWP391.Repositories.Interfaces;
using SWP391.Services.NotificationServices;
using SWP391.Services.TicketServices.Base;

namespace SWP391.Services.TicketServices
{
    /// <summary>
    /// Handles all read-only ticket operations:
    /// - Get ticket by code
    /// - Get overdue tickets
    /// - Check for duplicates
    /// </summary>
    public class TicketQueryService : BaseTicketService
    {
        private readonly TicketValidationService _validationService;

        public TicketQueryService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            ILogger<TicketQueryService> logger,
            TicketValidationService validationService)
            : base(unitOfWork, mapper, notificationService, logger)
        {
            _validationService = validationService;
        }

        /// <summary>
        /// Retrieves a single ticket by its unique code.
        /// </summary>
        public async Task<TicketDto> GetTicketByCodeAsync(string ticketCode)
        {
            var ticket = await UnitOfWork.TicketRepository.GetTicketByCodeAsync(ticketCode);
            return Mapper.Map<TicketDto>(ticket);
        }
    }
}