using SWP391.Services.Authentication;
using SWP391.Services.CategoryServices;
using SWP391.Services.Email;
using SWP391.Services.JWT;
using SWP391.Services.LocationServices;

namespace SWP391.Services.Application
{
    public class ApplicationServices : IApplicationServices
    {
        public IAuthenticationService AuthenticationService { get; }
        public IEmailService EmailService { get; }
        public IJwtService JwtService { get; }
        public ILocationService LocationService { get; }
        public ICategoryService CategoryService { get; }
        public ApplicationServices(
            IAuthenticationService authenticationService,
            IEmailService emailService,
            IJwtService jwtService,
            ILocationService locationService,
            ICategoryService categoryService)
        {
            AuthenticationService = authenticationService;
            EmailService = emailService;
            JwtService = jwtService;
            LocationService = locationService;
            CategoryService = categoryService;
        }
    }
}