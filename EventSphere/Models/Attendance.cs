namespace EventSphere.Models
{
    /// <summary>
    /// Represents attendance for a registration.
    /// </summary>
    public class Attendance
    {
        public int AttendanceId { get; set; }
        public int RegistrationId { get; set; }
        // Status: Present, Absent
        public string AttendanceStatus { get; set; } = "Absent";
        public DateTime? MarkedAt { get; set; }
        public string? MarkedBy { get; set; }
    }
}
