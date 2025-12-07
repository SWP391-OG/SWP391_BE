using SWP391.Services.Authentication;
using SWP391.Services.DepartmentServices;
using SWP391.Services.CategoryServices;
using SWP391.Services.Email;
using SWP391.Services.JWT;
using SWP391.Services.LocationServices;
using SWP391.Services.RoleServices;
using SWP391.Services.TicketServices;

namespace SWP391.Services.Application
{
    public class ApplicationServices : IApplicationServices
    {
        public IAuthenticationService AuthenticationService { get; }
        public IEmailService EmailService { get; }
        public IJwtService JwtService { get; }
        public ILocationService LocationService { get; }
        public ICategoryService CategoryService { get; }
        public IDepartmentService DepartmentService {  get; }
        public IRoleService RoleService { get; }
        public ITicketService TicketService { get; set; }
        public ApplicationServices(
            IAuthenticationService authenticationService,
            IEmailService emailService,
            IJwtService jwtService,
            ILocationService locationService,
            ICategoryService categoryService,
            IDepartmentService departmentService,
            IRoleService roleService,
            ITicketService ticketService)
        {
            AuthenticationService = authenticationService;
            EmailService = emailService;
            JwtService = jwtService;
            LocationService = locationService;
            CategoryService = categoryService;
            DepartmentService = departmentService;
            RoleService = roleService;
            TicketService = ticketService;
        }
    }
}