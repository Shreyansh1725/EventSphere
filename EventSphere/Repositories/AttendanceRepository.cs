using EventSphere.Data;
using EventSphere.Models;
using EventSphere.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EventSphere.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly DbConnectionFactory _factory;

        public AttendanceRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<AttendanceRecord> GetByEventId(int eventId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT r.RegistrationId, u.FullName AS StudentName,
                    ISNULL(u.EnrollmentNumber,'N/A') AS EnrollmentNumber,
                    r.RegistrationCode,
                    ISNULL(a.AttendanceStatus,'Absent') AS AttendanceStatus,
                    a.AttendanceId
                FROM Registrations r
                JOIN Users u ON r.UserId = u.UserId
                LEFT JOIN Attendance a ON a.RegistrationId = r.RegistrationId
                WHERE r.EventId = @EventId AND r.Status = 'Registered'
                ORDER BY u.FullName", conn);
            cmd.Parameters.AddWithValue("@EventId", eventId);

            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);

            var list = new List<AttendanceRecord>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new AttendanceRecord
                {
                    RegistrationId = (int)row["RegistrationId"],
                    StudentName = row["StudentName"].ToString()!,
                    EnrollmentNumber = row["EnrollmentNumber"].ToString()!,
                    RegistrationCode = row["RegistrationCode"].ToString()!,
                    AttendanceStatus = row["AttendanceStatus"].ToString()!,
                    AttendanceId = row["AttendanceId"] == DBNull.Value ? null : (int?)row["AttendanceId"]
                });
            }
            return list;
        }

        public bool MarkAttendance(int registrationId, string status, string markedBy)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var checkCmd = new SqlCommand(
                "SELECT COUNT(1) FROM Attendance WHERE RegistrationId = @RegId", conn);
            checkCmd.Parameters.AddWithValue("@RegId", registrationId);
            int exists = (int)checkCmd.ExecuteScalar()!;

            if (exists > 0)
            {
                using var updateCmd = new SqlCommand(@"
                    UPDATE Attendance SET AttendanceStatus=@Status, MarkedAt=GETDATE(), MarkedBy=@By
                    WHERE RegistrationId=@RegId", conn);
                updateCmd.Parameters.AddWithValue("@Status", status);
                updateCmd.Parameters.AddWithValue("@By", markedBy);
                updateCmd.Parameters.AddWithValue("@RegId", registrationId);
                return updateCmd.ExecuteNonQuery() > 0;
            }
            else
            {
                using var insertCmd = new SqlCommand(@"
                    INSERT INTO Attendance (RegistrationId, AttendanceStatus, MarkedAt, MarkedBy)
                    VALUES (@RegId, @Status, GETDATE(), @By)", conn);
                insertCmd.Parameters.AddWithValue("@RegId", registrationId);
                insertCmd.Parameters.AddWithValue("@Status", status);
                insertCmd.Parameters.AddWithValue("@By", markedBy);
                return insertCmd.ExecuteNonQuery() > 0;
            }
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

        public int GetTotalAttendance()
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM Attendance WHERE AttendanceStatus = 'Present'", conn);
            return (int)cmd.ExecuteScalar()!;
        }
    }
}
