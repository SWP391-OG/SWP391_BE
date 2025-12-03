using SWP391.Services.Authentication;
using SWP391.Services.Email;
using SWP391.Services.JWT;

namespace SWP391.Services.Application
{
    public interface IApplicationServices
    {
        IAuthenticationService AuthenticationService { get; }
        IEmailService EmailService { get; }
        IJwtService JwtService { get; }
    }
}