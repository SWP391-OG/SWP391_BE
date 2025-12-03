using SWP391.Services.Authentication;
using SWP391.Services.Email;
using SWP391.Services.JWT;

namespace SWP391.Services.Application
{
    public class ApplicationServices : IApplicationServices
    {
        public IAuthenticationService AuthenticationService { get; }
        public IEmailService EmailService { get; }
        public IJwtService JwtService { get; }

        public ApplicationServices(
            IAuthenticationService authenticationService,
            IEmailService emailService,
            IJwtService jwtService)
        {
            AuthenticationService = authenticationService;
            EmailService = emailService;
            JwtService = jwtService;
        }
    }
}