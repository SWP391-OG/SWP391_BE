
using SWP391.Contracts.Authentication;

namespace SWP391.Services.Authentication
{
    public interface IAuthenticationService
    {
        Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto request);
        Task<(bool Success, string Message)> VerifyEmailAsync(VerifyEmailRequestDto request);
        Task<(bool Success, string Message)> ResendVerificationAsync(ResendVerificationRequestDto request);
        Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<(bool Success, string Message, AuthResponseDto Data)> LoginAsync(LoginRequestDto request);
    }
}