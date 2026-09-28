using EventSphere.Data;
using EventSphere.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EventSphere.Repositories
{
    /// <summary>
    /// ADO.NET implementation for Registration CRUD.
    /// </summary>
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly DbConnectionFactory _factory;

        public RegistrationRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<Registration> GetByUserId(int userId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT r.*, e.EventName, e.EventDate, e.Venue, u.FullName AS StudentName,
                    u.Email, u.EnrollmentNumber,
                    ISNULL(a.AttendanceStatus, 'Not Marked') AS AttendanceStatus
                FROM Registrations r
                JOIN Events e ON r.EventId = e.EventId
                JOIN Users u ON r.UserId = u.UserId
                LEFT JOIN Attendance a ON a.RegistrationId = r.RegistrationId
                WHERE r.UserId = @UserId
                ORDER BY r.RegistrationDate DESC", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = cmd.ExecuteReader();
            var list = new List<Registration>();
            while (reader.Read())
                list.Add(MapRegistration(reader));
            return list;
        }

        public List<Registration> GetAll(int? eventId = null, string? status = null, string? searchTerm = null)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            var sql = @"
                SELECT r.*, e.EventName, e.EventDate, e.Venue, u.FullName AS StudentName,
                    u.Email, u.EnrollmentNumber,
                    ISNULL(a.AttendanceStatus, 'Not Marked') AS AttendanceStatus
                FROM Registrations r
                JOIN Events e ON r.EventId = e.EventId
                JOIN Users u ON r.UserId = u.UserId
                LEFT JOIN Attendance a ON a.RegistrationId = r.RegistrationId
                WHERE 1=1";
            if (eventId.HasValue) sql += " AND r.EventId = @EventId";
            if (!string.IsNullOrEmpty(status)) sql += " AND r.Status = @Status";
            if (!string.IsNullOrEmpty(searchTerm)) sql += " AND (u.FullName LIKE @Search OR u.Email LIKE @Search OR u.EnrollmentNumber LIKE @Search)";
            sql += " ORDER BY r.RegistrationDate DESC";

            using var cmd = new SqlCommand(sql, conn);
            if (eventId.HasValue) cmd.Parameters.AddWithValue("@EventId", eventId.Value);
            if (!string.IsNullOrEmpty(status)) cmd.Parameters.AddWithValue("@Status", status);
            if (!string.IsNullOrEmpty(searchTerm)) cmd.Parameters.AddWithValue("@Search", $"%{searchTerm}%");

            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            var list = new List<Registration>();
            foreach (DataRow row in dt.Rows)
                list.Add(MapRegistrationFromRow(row));
            return list;
        }

        public Registration? GetById(int registrationId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT r.*, e.EventName, e.EventDate, e.Venue, u.FullName AS StudentName,
                    u.Email, u.EnrollmentNumber,
                    ISNULL(a.AttendanceStatus, 'Not Marked') AS AttendanceStatus
                FROM Registrations r
                JOIN Events e ON r.EventId = e.EventId
                JOIN Users u ON r.UserId = u.UserId
                LEFT JOIN Attendance a ON a.RegistrationId = r.RegistrationId
                WHERE r.RegistrationId = @RegistrationId", conn);
            cmd.Parameters.AddWithValue("@RegistrationId", registrationId);
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return MapRegistration(reader);
            return null;
        }

        public bool IsAlreadyRegistered(int userId, int eventId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(1) FROM Registrations
                WHERE UserId = @UserId AND EventId = @EventId AND Status != 'Cancelled'", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@EventId", eventId);
            return (int)cmd.ExecuteScalar()! > 0;
        }

        public int Create(Registration reg)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO Registrations (EventId, UserId, RegistrationCode, RegistrationDate, Status)
                OUTPUT INSERTED.RegistrationId
                VALUES (@EventId, @UserId, @Code, GETDATE(), 'Registered')", conn);
            cmd.Parameters.AddWithValue("@EventId", reg.EventId);
            cmd.Parameters.AddWithValue("@UserId", reg.UserId);
            cmd.Parameters.AddWithValue("@Code", reg.RegistrationCode);
            return (int)cmd.ExecuteScalar()!;
        }

        public bool UpdateStatus(int registrationId, string status)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "UPDATE Registrations SET Status = @Status WHERE RegistrationId = @Id", conn);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Id", registrationId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public int GetRegistrationCountByUser(int userId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM Registrations WHERE UserId = @UserId AND Status != 'Cancelled'", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            return (int)cmd.ExecuteScalar()!;
        }

        public int GetAttendedCountByUser(int userId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(1) FROM Attendance a
                JOIN Registrations r ON a.RegistrationId = r.RegistrationId
                WHERE r.UserId = @UserId AND a.AttendanceStatus = 'Present'", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            return (int)cmd.ExecuteScalar()!;
        }

        private static Registration MapRegistration(SqlDataReader r)
        {
            return new Registration
            {
                RegistrationId = r.GetInt32(r.GetOrdinal("RegistrationId")),
                EventId = r.GetInt32(r.GetOrdinal("EventId")),
                UserId = r.GetInt32(r.GetOrdinal("UserId")),
                RegistrationCode = r.GetString(r.GetOrdinal("RegistrationCode")),
                RegistrationDate = r.GetDateTime(r.GetOrdinal("RegistrationDate")),
                Status = r.GetString(r.GetOrdinal("Status")),
                EventName = r.IsDBNull(r.GetOrdinal("EventName")) ? null : r.GetString(r.GetOrdinal("EventName")),
                EventDate = r.IsDBNull(r.GetOrdinal("EventDate")) ? null : r.GetDateTime(r.GetOrdinal("EventDate")),
                Venue = r.IsDBNull(r.GetOrdinal("Venue")) ? null : r.GetString(r.GetOrdinal("Venue")),
                StudentName = r.IsDBNull(r.GetOrdinal("StudentName")) ? null : r.GetString(r.GetOrdinal("StudentName")),
                Email = r.IsDBNull(r.GetOrdinal("Email")) ? null : r.GetString(r.GetOrdinal("Email")),
                EnrollmentNumber = r.IsDBNull(r.GetOrdinal("EnrollmentNumber")) ? null : r.GetString(r.GetOrdinal("EnrollmentNumber")),
                AttendanceStatus = r.IsDBNull(r.GetOrdinal("AttendanceStatus")) ? null : r.GetString(r.GetOrdinal("AttendanceStatus"))
            };
        }

        private static Registration MapRegistrationFromRow(DataRow row)
        {
            return new Registration
            {
                RegistrationId = (int)row["RegistrationId"],
                EventId = (int)row["EventId"],
                UserId = (int)row["UserId"],
                RegistrationCode = row["RegistrationCode"].ToString()!,
                RegistrationDate = (DateTime)row["RegistrationDate"],
                Status = row["Status"].ToString()!,
                EventName = row["EventName"] == DBNull.Value ? null : row["EventName"].ToString(),
                EventDate = row["EventDate"] == DBNull.Value ? null : (DateTime?)row["EventDate"],
                Venue = row["Venue"] == DBNull.Value ? null : row["Venue"].ToString(),
                StudentName = row["StudentName"] == DBNull.Value ? null : row["StudentName"].ToString(),
                Email = row["Email"] == DBNull.Value ? null : row["Email"].ToString(),
                EnrollmentNumber = row["EnrollmentNumber"] == DBNull.Value ? null : row["EnrollmentNumber"].ToString(),
                AttendanceStatus = row["AttendanceStatus"] == DBNull.Value ? null : row["AttendanceStatus"].ToString()
            };
        }
    }
}
