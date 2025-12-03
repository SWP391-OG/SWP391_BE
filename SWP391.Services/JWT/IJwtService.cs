using SWP391.Repositories.Models;

namespace SWP391.Services.JWT
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        DateTime GetTokenExpirationTime();
    }
}