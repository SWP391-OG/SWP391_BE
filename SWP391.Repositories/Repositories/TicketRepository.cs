using SWP391.Repositories.Basic;
using SWP391.Repositories.DBContext;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWP391.Repositories.Repositories
{
    public class TicketRepository : GenericRepository<Ticket>, ITicketRepository
    {
        public TicketRepository() => _context ??= new FPTechnicalContext();

        public TicketRepository(FPTechnicalContext context) => _context = context;
    }

}
