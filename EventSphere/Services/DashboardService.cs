using EventSphere.Data;
using EventSphere.Models;
using EventSphere.ViewModels;
using Microsoft.Data.SqlClient;

namespace EventSphere.Services
{
    /// <summary>
    /// Service for aggregated dashboard statistics.
    /// Demonstrates: Service layer, Dependency Injection.
    /// </summary>
    public class DashboardService
    {
        private readonly DbConnectionFactory _factory;

        public DashboardService(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public AdminDashboardViewModel GetAdminStats()
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            var vm = new AdminDashboardViewModel();

            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Events", conn))
                vm.TotalEvents = (int)cmd.ExecuteScalar()!;
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Events WHERE Status='Published'", conn))
                vm.PublishedEvents = (int)cmd.ExecuteScalar()!;
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Role='Student' AND IsActive=1", conn))
                vm.TotalStudents = (int)cmd.ExecuteScalar()!;
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Registrations WHERE Status='Registered'", conn))
                vm.TotalRegistrations = (int)cmd.ExecuteScalar()!;
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Events WHERE EventDate >= CAST(GETDATE() AS DATE)", conn))
                vm.UpcomingEventsCount = (int)cmd.ExecuteScalar()!;
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Attendance WHERE AttendanceStatus='Present'", conn))
                vm.TotalAttendance = (int)cmd.ExecuteScalar()!;

            // Recent registrations
            using (var cmd = new SqlCommand(@"
                SELECT TOP 5 r.RegistrationId, r.EventId, r.UserId, r.RegistrationCode,
                    r.RegistrationDate, r.Status,
                    e.EventName, e.EventDate, e.Venue,
                    u.FullName AS StudentName, u.Email, u.EnrollmentNumber,
                    'Not Marked' AS AttendanceStatus
                FROM Registrations r
                JOIN Events e ON r.EventId = e.EventId
                JOIN Users u ON r.UserId = u.UserId
                ORDER BY r.RegistrationDate DESC", conn))
            {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    vm.RecentRegistrations.Add(new Registration
                    {
                        RegistrationId = reader.GetInt32(0),
                        EventId = reader.GetInt32(1),
                        UserId = reader.GetInt32(2),
                        RegistrationCode = reader.GetString(3),
                        RegistrationDate = reader.GetDateTime(4),
                        Status = reader.GetString(5),
                        EventName = reader.IsDBNull(6) ? null : reader.GetString(6),
                        EventDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                        Venue = reader.IsDBNull(8) ? null : reader.GetString(8),
                        StudentName = reader.IsDBNull(9) ? null : reader.GetString(9),
                        Email = reader.IsDBNull(10) ? null : reader.GetString(10),
                        EnrollmentNumber = reader.IsDBNull(11) ? null : reader.GetString(11),
                        AttendanceStatus = reader.GetString(12)
                    });
                }
            }

            // Upcoming events
            using (var cmd = new SqlCommand(@"
                SELECT TOP 5 e.EventId, e.EventName, e.Description, e.CategoryId, e.Venue,
                    e.EventDate, e.StartTime, e.EndTime, e.Organizer, e.MaximumParticipants,
                    e.AvailableSeats, e.RegistrationDeadline, e.ImagePath, e.Status, e.CreatedAt,
                    c.CategoryName
                FROM Events e
                LEFT JOIN EventCategories c ON e.CategoryId = c.CategoryId
                WHERE e.EventDate >= CAST(GETDATE() AS DATE) AND e.Status = 'Published'
                ORDER BY e.EventDate ASC", conn))
            {
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    vm.UpcomingEvents.Add(new Event
                    {
                        EventId = reader.GetInt32(0),
                        EventName = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                        CategoryId = reader.GetInt32(3),
                        Venue = reader.GetString(4),
                        EventDate = reader.GetDateTime(5),
                        StartTime = reader.IsDBNull(6) ? null : reader.GetTimeSpan(6),
                        EndTime = reader.IsDBNull(7) ? null : reader.GetTimeSpan(7),
                        Organizer = reader.IsDBNull(8) ? null : reader.GetString(8),
                        MaximumParticipants = reader.GetInt32(9),
                        AvailableSeats = reader.GetInt32(10),
                        RegistrationDeadline = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                        ImagePath = reader.IsDBNull(12) ? null : reader.GetString(12),
                        Status = reader.GetString(13),
                        CreatedAt = reader.GetDateTime(14),
                        CategoryName = reader.IsDBNull(15) ? null : reader.GetString(15)
                    });
                }
            }

            return vm;
        }

        public StudentDashboardViewModel GetStudentStats(int userId, string studentName)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            var vm = new StudentDashboardViewModel { StudentName = studentName };

            using (var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM Registrations WHERE UserId=@UserId AND Status='Registered'", conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                vm.TotalRegisteredEvents = (int)cmd.ExecuteScalar()!;
            }

            using (var cmd = new SqlCommand(@"
                SELECT COUNT(1) FROM Attendance a
                JOIN Registrations r ON a.RegistrationId = r.RegistrationId
                WHERE r.UserId=@UserId AND a.AttendanceStatus='Present'", conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                vm.TotalAttendedEvents = (int)cmd.ExecuteScalar()!;
            }

            using (var cmd = new SqlCommand(@"
                SELECT COUNT(1) FROM Certificates c
                JOIN Registrations r ON c.RegistrationId = r.RegistrationId
                WHERE r.UserId = @UserId", conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                vm.AvailableCertificates = (int)cmd.ExecuteScalar()!;
            }

            using (var cmd = new SqlCommand(@"
                SELECT COUNT(1) FROM Notifications
                WHERE (UserId=@UserId OR UserId IS NULL) AND IsRead=0", conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                vm.UnreadNotifications = (int)cmd.ExecuteScalar()!;
            }

            using (var cmd = new SqlCommand(@"
                SELECT TOP 5 * FROM Notifications
                WHERE (UserId=@UserId OR UserId IS NULL)
                ORDER BY CreatedAt DESC", conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    vm.RecentNotifications.Add(new Notification
                    {
                        NotificationId = reader.GetInt32(0),
                        UserId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                        Title = reader.GetString(2),
                        Message = reader.GetString(3),
                        IsRead = reader.GetBoolean(4),
                        CreatedAt = reader.GetDateTime(5)
                    });
                }
            }

            return vm;
        }
    }
}
