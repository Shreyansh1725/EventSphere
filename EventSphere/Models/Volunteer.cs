namespace EventSphere.Models
{
    /// <summary>
    /// Represents a volunteer assigned to an event.
    /// </summary>
    public class Volunteer
    {
        public int VolunteerId { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Active";

        // Populated by JOINs
        public string? StudentName { get; set; }
        public string? Email { get; set; }
        public string? EnrollmentNumber { get; set; }
        public string? EventName { get; set; }
        public DateTime? EventDate { get; set; }
    }
}
