using AutoMapper;
using SWP391.Contracts.Authentication;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Models;
using SWP391.Services.Email;
using SWP391.Services.JWT;
using System.Security.Cryptography;
using System.Text;

namespace SWP391.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;  

        public AuthenticationService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IJwtService jwtService,
            IMapper mapper)  
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _jwtService = jwtService;
            _mapper = mapper;  
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequestDto request)
        {
            // Check if email already exists
            if (await _unitOfWork.UserRepository.EmailExistsAsync(request.Email))
            {
                return (false, "Email already exists");
            }

            // Generate verification code
            var verificationCode = GenerateVerificationCode();

            // Create verification code record
            var verificationRecord = new VerificationCode
            {
                Email = request.Email,
                Code = verificationCode,
                Type = "EmailVerification",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            };

            await _unitOfWork.VerificationCodeRepository.CreateAsync(verificationRecord);

            // Use AutoMapper to create User from RegisterRequestDto
            var user = _mapper.Map<User>(request);
            
            // Set properties that AutoMapper ignores
            user.PasswordHash = HashPassword(request.Password);
            user.UserCode = GenerateUserCode();
            user.Status = "INACTIVE";
            user.RoleId = 3;
            user.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.UserRepository.CreateAsync(user);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Send verification email
            await _emailService.SendVerificationEmailAsync(request.Email, verificationCode);

            return (true, "Registration successful. Please check your email for verification code.");
        }

        public async Task<(bool Success, string Message)> VerifyEmailAsync(VerifyEmailRequestDto request)
        {
            // Validate verification code
            var verificationRecord = await _unitOfWork.VerificationCodeRepository
                .GetValidCodeAsync(request.Email, request.VerificationCode, "EmailVerification");

            if (verificationRecord == null)
            {
                return (false, "Invalid or expired verification code");
            }

            // Get user
            var user = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return (false, "User not found");
            }

            // Update user status to ACTIVE (changed from "Active")
            user.Status = "ACTIVE";  
            await _unitOfWork.UserRepository.UpdateAsync(user);

            // Mark verification code as used
            verificationRecord.IsUsed = true;
            await _unitOfWork.VerificationCodeRepository.UpdateAsync(verificationRecord);

            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "Email verified successfully. You can now login.");
        }

        public async Task<(bool Success, string Message)> ResendVerificationAsync(ResendVerificationRequestDto request)
        {
            // Check if user exists
            var user = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return (false, "User not found");
            }

            if (user.Status == "ACTIVE")  
            {
                return (false, "Email is already verified");
            }

            // Invalidate old codes
            await _unitOfWork.VerificationCodeRepository.InvalidateAllCodesAsync(request.Email, "EmailVerification");

            // Generate new verification code
            var verificationCode = GenerateVerificationCode();

            var verificationRecord = new VerificationCode
            {
                Email = request.Email,
                Code = verificationCode,
                Type = "EmailVerification",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            };

            await _unitOfWork.VerificationCodeRepository.CreateAsync(verificationRecord);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Send verification email
            await _emailService.SendVerificationEmailAsync(request.Email, verificationCode);

            return (true, "Verification code resent successfully. Please check your email.");
        }

        public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            // Check if user exists
            var user = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that user doesn't exist for security reasons
                return (true, "If the email exists, a password reset code has been sent.");
            }

            if (user.Status != "ACTIVE")  
            {
                return (false, "Account is not active. Please verify your email first.");
            }

            // Invalidate old reset codes
            await _unitOfWork.VerificationCodeRepository.InvalidateAllCodesAsync(request.Email, "PasswordReset");

            // Generate reset code
            var resetCode = GenerateVerificationCode();

            var verificationRecord = new VerificationCode
            {
                Email = request.Email,
                Code = resetCode,
                Type = "PasswordReset",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false
            };

            await _unitOfWork.VerificationCodeRepository.CreateAsync(verificationRecord);
            await _unitOfWork.SaveChangesWithTransactionAsync();

            // Send reset email
            await _emailService.SendPasswordResetEmailAsync(request.Email, resetCode);

            return (true, "If the email exists, a password reset code has been sent.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            // Validate reset code
            var verificationRecord = await _unitOfWork.VerificationCodeRepository
                .GetValidCodeAsync(request.Email, request.ResetCode, "PasswordReset");

            if (verificationRecord == null)
            {
                return (false, "Invalid or expired reset code");
            }

            // Get user
            var user = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return (false, "User not found");
            }

            // Update password
            user.PasswordHash = HashPassword(request.NewPassword);
            await _unitOfWork.UserRepository.UpdateAsync(user);

            // Mark reset code as used
            verificationRecord.IsUsed = true;
            await _unitOfWork.VerificationCodeRepository.UpdateAsync(verificationRecord);

            await _unitOfWork.SaveChangesWithTransactionAsync();

            return (true, "Password reset successfully. You can now login with your new password.");
        }

        public async Task<(bool Success, string Message, AuthResponseDto Data)> LoginAsync(LoginRequestDto request)
        {
            // Get user by email
            var user = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return (false, "Invalid email or password", null);
            }

            // Check if account is active
            if (user.Status != "ACTIVE")
            {
                return (false, "Account is not active. Please verify your email first.", null);
            }

            // Verify password
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return (false, "Invalid email or password", null);
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);
            var expiresAt = _jwtService.GetTokenExpirationTime();

            // ✅ Use AutoMapper to create AuthResponseDto
            var response = _mapper.Map<AuthResponseDto>(user);
            
            // Set properties that AutoMapper ignores
            response.Token = token;
            response.ExpiresAt = expiresAt;

            return (true, "Login successful", response);
        }

        #region Helper Methods

        private string GenerateVerificationCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private string GenerateUserCode()
        {
            return "FPT" + DateTime.UtcNow.Ticks.ToString().Substring(8);
        }

        private string HashPassword(string password)
            => BCrypt.Net.BCrypt.HashPassword(password);

        private bool VerifyPassword(string password, string hash)
            => BCrypt.Net.BCrypt.Verify(password, hash);

        #endregion
    }
}