using SWP391.Services.Authentication;
using SWP391.Services.CategoryServices;
using SWP391.Services.DepartmentServices;
using SWP391.Services.Email;
using SWP391.Services.JWT;
using SWP391.Services.LocationServices;
using SWP391.Services.NotificationServices;
using SWP391.Services.RoleServices;
using SWP391.Services.TicketServices;
using SWP391.Services.UserServices;

namespace SWP391.Services.Application
{
    public interface IApplicationServices
    {
        IAuthenticationService AuthenticationService { get; }
        IEmailService EmailService { get; }
        IJwtService JwtService { get; }
        ILocationService LocationService { get; }
        ICategoryService CategoryService { get; }
        IDepartmentService DepartmentService { get; }
        IRoleService RoleService { get; }
        ITicketService TicketService { get; }
        IUserService UserService { get; }
        INotificationService NotificationService { get; }
    }
}