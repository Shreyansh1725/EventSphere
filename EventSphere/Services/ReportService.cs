using EventSphere.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EventSphere.Services
{
    /// <summary>
    /// Service for generating reports using ADO.NET DataSet/DataTable.
    /// Demonstrates: DataSet, DataTable usage.
    /// </summary>
    public class ReportService
    {
        private readonly DbConnectionFactory _factory;

        public ReportService(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public DataSet GetFullReportDataSet()
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            var ds = new DataSet("EventSphereReports");

            using (var cmd = new SqlCommand(@"
                SELECT e.EventName, e.EventDate, e.Venue,
                    COUNT(r.RegistrationId) AS TotalRegistrations,
                    SUM(CASE WHEN a.AttendanceStatus = 'Present' THEN 1 ELSE 0 END) AS TotalPresent
                FROM Events e
                LEFT JOIN Registrations r ON e.EventId = r.EventId AND r.Status = 'Registered'
                LEFT JOIN Attendance a ON r.RegistrationId = a.RegistrationId
                GROUP BY e.EventId, e.EventName, e.EventDate, e.Venue
                ORDER BY e.EventDate DESC", conn))
            {
                var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds, "EventRegistrations");
            }

            using (var cmd = new SqlCommand(@"
                SELECT u.Department, COUNT(r.RegistrationId) AS TotalRegistrations
                FROM Users u
                LEFT JOIN Registrations r ON u.UserId = r.UserId AND r.Status = 'Registered'
                WHERE u.Role = 'Student'
                GROUP BY u.Department
                ORDER BY TotalRegistrations DESC", conn))
            {
                var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds, "DepartmentParticipation");
            }

            using (var cmd = new SqlCommand(@"
                SELECT TOP 5 e.EventName, COUNT(r.RegistrationId) AS Registrations
                FROM Events e
                LEFT JOIN Registrations r ON e.EventId = r.EventId
                GROUP BY e.EventId, e.EventName
                ORDER BY Registrations DESC", conn))
            {
                var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(ds, "PopularEvents");
            }

            return ds;
        }

        public DataTable GetEventWiseRegistrations()
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT e.EventName, e.EventDate, c.CategoryName, e.Venue,
                    COUNT(r.RegistrationId) AS TotalRegistrations,
                    e.MaximumParticipants,
                    SUM(CASE WHEN a.AttendanceStatus = 'Present' THEN 1 ELSE 0 END) AS Attended
                FROM Events e
                LEFT JOIN EventCategories c ON e.CategoryId = c.CategoryId
                LEFT JOIN Registrations r ON e.EventId = r.EventId AND r.Status = 'Registered'
                LEFT JOIN Attendance a ON r.RegistrationId = a.RegistrationId
                GROUP BY e.EventId, e.EventName, e.EventDate, c.CategoryName, e.Venue, e.MaximumParticipants
                ORDER BY e.EventDate DESC", conn);
            var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public DataTable GetDepartmentParticipation()
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT ISNULL(u.Department, 'Unknown') AS Department,
                    COUNT(DISTINCT u.UserId) AS TotalStudents,
                    COUNT(r.RegistrationId) AS TotalRegistrations
                FROM Users u
                LEFT JOIN Registrations r ON u.UserId = r.UserId AND r.Status = 'Registered'
                WHERE u.Role = 'Student'
                GROUP BY u.Department
                ORDER BY TotalRegistrations DESC", conn);
            var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }
    }
}
