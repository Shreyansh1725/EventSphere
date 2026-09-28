using EventSphere.Models;

namespace EventSphere.ViewModels
{
    public class AttendanceViewModel
    {
        public List<Event> Events { get; set; } = new();
        public int? SelectedEventId { get; set; }
        public string? SelectedEventName { get; set; }
        public List<AttendanceRecord> AttendanceRecords { get; set; } = new();
        public List<Volunteer> Volunteers { get; set; } = new();
        public List<User> AvailableStudents { get; set; } = new();
    }

    public class AttendanceRecord
    {
        public int RegistrationId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string EnrollmentNumber { get; set; } = string.Empty;
        public string RegistrationCode { get; set; } = string.Empty;
        public string AttendanceStatus { get; set; } = "Absent";
        public int? AttendanceId { get; set; }
    }
}
