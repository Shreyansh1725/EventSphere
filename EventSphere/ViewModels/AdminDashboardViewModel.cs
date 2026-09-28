using EventSphere.Models;

namespace EventSphere.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int PublishedEvents { get; set; }
        public int TotalStudents { get; set; }
        public int TotalRegistrations { get; set; }
        public int UpcomingEventsCount { get; set; }
        public int TotalAttendance { get; set; }
        public List<Registration> RecentRegistrations { get; set; } = new();
        public List<Event> UpcomingEvents { get; set; } = new();
    }
}
