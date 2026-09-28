using EventSphere.Models;

namespace EventSphere.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public int TotalRegisteredEvents { get; set; }
        public int TotalAttendedEvents { get; set; }
        public int AvailableCertificates { get; set; }
        public int UnreadNotifications { get; set; }
        public List<Event> UpcomingEvents { get; set; } = new();
        public List<Notification> RecentNotifications { get; set; } = new();
    }
}
