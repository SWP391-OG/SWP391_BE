namespace SWP391.WebAPI.Constants
{
    public static class ApiMessages
    {
        // Validation Messages
        public const string INVALID_REQUEST_DATA = "Invalid request data";
        public const string INVALID_USER_AUTHENTICATION = "Invalid user authentication";
        public const string UNAUTHORIZED_ACCESS = "You do not have permission to access this resource";

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

        // Ticket Messages
        public const string TICKET_RETRIEVED_SUCCESS = "Ticket retrieved successfully";
        public const string TICKETS_RETRIEVED_SUCCESS = "Tickets retrieved successfully";
        public const string TICKET_NOT_FOUND = "Ticket not found";
        public const string TICKET_CREATED_SUCCESS = "Ticket created successfully";
        public const string TICKET_UPDATED_SUCCESS = "Ticket updated successfully";
        public const string TICKET_CANCELLED_SUCCESS = "Ticket cancelled successfully";
        public const string TICKET_STATUS_UPDATED_SUCCESS = "Ticket status updated successfully";
        public const string TICKET_ASSIGNED_AUTO_SUCCESS = "Ticket automatically assigned";
        public const string TICKET_ASSIGNED_MANUAL_SUCCESS = "Ticket manually assigned";
        public const string FEEDBACK_SUBMITTED_SUCCESS = "Feedback submitted successfully and ticket is now closed";
        public const string STAFF_WORKLOAD_RETRIEVED_SUCCESS = "Staff workload retrieved successfully";
        public const string TICKET_OVERDUE = "Ticket has exceeded its SLA deadline";
        public const string DUPLICATE_TICKET_DETECTED = "A similar ticket already exists";
        public const string TICKET_ESCALATED_SUCCESS = "Ticket escalated successfully";
        public const string OVERDUE_TICKETS_RETRIEVED = "Overdue tickets retrieved successfully";

        // Ticket Validation Messages
        public const string STAFF_CODE_REQUIRED = "Staff code is required for manual assignment";
        public const string ONLY_STUDENTS_CAN_CREATE_TICKETS = "Only students can create tickets";
        public const string ONLY_ADMIN_CAN_ASSIGN_TICKETS = "Only administrators can assign tickets";
        public const string ONLY_STAFF_CAN_UPDATE_STATUS = "Only staff members can update ticket status";
        public const string ONLY_STUDENTS_CAN_PROVIDE_FEEDBACK = "Only students can provide feedback on their tickets";
        public const string TICKET_ALREADY_CANCELLED = "Ticket is already cancelled";
        public const string TICKET_ALREADY_CLOSED = "Ticket is already closed and cannot be cancelled";
        public const string ONLY_NEW_TICKETS_CAN_BE_CANCELLED_BY_STUDENT = "Only NEW tickets can be cancelled by students";
        public const string ONLY_TICKET_OWNER_CAN_CANCEL = "You can only cancel your own tickets";
        public const string ADMIN_CAN_CANCEL_ANY_TICKET = "Administrators can cancel any ticket";

        // Notification Messages
        public const string NOTIFICATIONS_RETRIEVED_SUCCESS = "Notifications retrieved successfully";
        public const string NOTIFICATION_MARKED_AS_READ = "Notification marked as read";
        public const string ALL_NOTIFICATIONS_MARKED_AS_READ = "All notifications marked as read";
        public const string NOTIFICATION_NOT_FOUND = "Notification not found";
        public const string UNREAD_COUNT_RETRIEVED = "Unread notification count retrieved successfully";
    }
}