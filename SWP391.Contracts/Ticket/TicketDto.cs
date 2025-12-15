namespace SWP391.Contracts.Ticket
{
    public class TicketDto
    {
        public string TicketCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        // Use codes instead of IDs for frontend
        public string RequesterCode { get; set; } = string.Empty;
        public string RequesterName { get; set; } = string.Empty;
        public string AssignedToCode { get; set; } = string.Empty;
        public string AssignedToName { get; set; } = string.Empty;
        public string ManagedByCode { get; set; } = string.Empty;
        public string ManagedByName { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? ResolveDeadline { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int? RatingStars { get; set; }
        public string RatingComment { get; set; } = string.Empty;
    }

    // Student creates ticket - using codes
    public class CreateTicketRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
    }

    // Admin assigns ticket (AUTOMATIC - just needs ticket code)
    public class AssignTicketRequestDto
    {
        public string ManualStaffCode { get; set; } = string.Empty; // Optional: Admin can override automatic assignment
    }

    // Update ticket status (generic for state transitions)
    public class UpdateTicketStatusDto
    {
        public string Status { get; set; } = string.Empty; // IN_PROGRESS, RESOLVED
        public string? ResolutionNotes { get; set; } // REQUIRED when transitioning to RESOLVED
    }

    // Student provides feedback
    public class TicketFeedbackDto
    {
        public int RatingStars { get; set; } // 1-5
        public string RatingComment { get; set; } = string.Empty;
    }

    // Update ticket details (Admin/Staff can update title, description)
    public class UpdateTicketDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    // Get staff workload for a department
    public class StaffWorkloadDto
    {
        public string StaffCode { get; set; } = string.Empty;
        public string StaffName { get; set; } = string.Empty;
        public int ActiveTicketCount { get; set; }
        public string DepartmentCode { get; set; } = string.Empty;
    }

    // Cancel ticket DTO with required reason
    public class CancelTicketRequestDto
    {
        public string Reason { get; set; } = string.Empty; // Required cancellation reason
    }
}