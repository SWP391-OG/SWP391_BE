using AutoMapper;
using Microsoft.Extensions.Logging;
using SWP391.Contracts.Ticket;
using SWP391.Repositories.Interfaces;
using SWP391.Services.NotificationServices;
using SWP391.Services.TicketServices.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        /// <summary>
        /// Gets all overdue tickets (Admin view).
        /// </summary>
        public async Task<List<TicketDto>> GetOverdueTicketsAsync()
        {
            var tickets = await UnitOfWork.TicketRepository.GetOverdueTicketsAsync();
            return Mapper.Map<List<TicketDto>>(tickets);
        }

        /// <summary>
        /// Checks for duplicate tickets before creation.
        /// </summary>
        public async Task<(bool HasDuplicates, List<TicketDto> PotentialDuplicates)> CheckForDuplicatesAsync(
            CreateTicketRequestDto dto, int requesterId)
        {
            var category = await UnitOfWork.CategoryRepository.GetCategoryByCodeAsync(dto.CategoryCode);
            if (category == null)
                return (false, new List<TicketDto>());

            var location = await UnitOfWork.LocationRepository.GetLocationByCodeAsync(dto.LocationCode);
            int? locationId = location?.Id;

            var createdAfter = DateTime.UtcNow.AddDays(-7);

            var duplicates = await UnitOfWork.TicketRepository.CheckForDuplicateTicketsAsync(
                requesterId, dto.Title, category.Id, locationId, createdAfter);

            var duplicateDtos = Mapper.Map<List<TicketDto>>(duplicates);
            return (duplicates.Any(), duplicateDtos);
        }
    }
}