namespace EventSphere.Models
{
    /// <summary>
    /// Represents a student's registration for an event.
    /// </summary>
    public class Registration
    {
        public int RegistrationId { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string RegistrationCode { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        // Status: Registered, Cancelled, Attended
        public string Status { get; set; } = "Registered";

        // Navigation properties (populated by JOIN queries)
        public string? EventName { get; set; }
        public DateTime? EventDate { get; set; }
        public string? Venue { get; set; }
        public string? StudentName { get; set; }
        public string? Email { get; set; }
        public string? EnrollmentNumber { get; set; }
        public string? AttendanceStatus { get; set; }
    }
}
