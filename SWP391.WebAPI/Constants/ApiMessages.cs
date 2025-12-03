namespace SWP391.WebAPI.Constants
{
    public static class ApiMessages
    {
        // Validation Messages
        public const string INVALID_REQUEST_DATA = "Invalid request data";
        
        // Authentication Messages
        public const string LOGIN_SUCCESS = "Login successful";
        public const string LOGIN_FAILED = "Invalid email or password";
        public const string ACCOUNT_NOT_ACTIVE = "Account is not active. Please verify your email first";
        
        // Registration Messages
        public const string REGISTRATION_SUCCESS = "Registration successful. Please check your email for verification code";
        public const string EMAIL_ALREADY_EXISTS = "Email already exists";
        
        // Email Verification Messages
        public const string EMAIL_VERIFIED = "Email verified successfully. You can now login";
        public const string INVALID_VERIFICATION_CODE = "Invalid or expired verification code";
        public const string VERIFICATION_CODE_RESENT = "Verification code resent successfully. Please check your email";
        public const string EMAIL_ALREADY_VERIFIED = "Email is already verified";
        
        // Password Reset Messages
        public const string PASSWORD_RESET_EMAIL_SENT = "If the email exists, a password reset code has been sent";
        public const string PASSWORD_RESET_SUCCESS = "Password reset successfully. You can now login with your new password";
        public const string INVALID_RESET_CODE = "Invalid or expired reset code";
        
        // General Messages
        public const string USER_NOT_FOUND = "User not found";
        public const string SUCCESS = "Success";
    }
}