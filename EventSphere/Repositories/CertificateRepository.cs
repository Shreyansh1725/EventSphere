using EventSphere.Data;
using EventSphere.Models;
using Microsoft.Data.SqlClient;

namespace EventSphere.Repositories
{
    public class CertificateRepository : ICertificateRepository
    {
        private readonly DbConnectionFactory _factory;

        public CertificateRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<Certificate> GetByUserId(int userId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT c.*, u.FullName AS StudentName, e.EventName, e.EventDate,
                    ISNULL(a.AttendanceStatus, 'Not Marked') AS AttendanceStatus,
                    r.RegistrationCode
                FROM Certificates c
                JOIN Registrations r ON c.RegistrationId = r.RegistrationId
                JOIN Users u ON r.UserId = u.UserId
                JOIN Events e ON r.EventId = e.EventId
                LEFT JOIN Attendance a ON a.RegistrationId = r.RegistrationId
                WHERE r.UserId = @UserId
                ORDER BY c.IssueDate DESC", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = cmd.ExecuteReader();
            var list = new List<Certificate>();
            while (reader.Read())
                list.Add(MapCertificate(reader));
            return list;
        }

        public Certificate? GetById(int certificateId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT c.*, u.FullName AS StudentName, e.EventName, e.EventDate,
                    ISNULL(a.AttendanceStatus, 'Not Marked') AS AttendanceStatus,
                    r.RegistrationCode
                FROM Certificates c
                JOIN Registrations r ON c.RegistrationId = r.RegistrationId
                JOIN Users u ON r.UserId = u.UserId
                JOIN Events e ON r.EventId = e.EventId
                LEFT JOIN Attendance a ON a.RegistrationId = r.RegistrationId
                WHERE c.CertificateId = @CertId", conn);
            cmd.Parameters.AddWithValue("@CertId", certificateId);
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return MapCertificate(reader);
            return null;
        }

        public int GetCountByUserId(int userId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(1) FROM Certificates c
                JOIN Registrations r ON c.RegistrationId = r.RegistrationId
                WHERE r.UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            return (int)cmd.ExecuteScalar()!;
        }

        public bool CreateIfNotExists(int registrationId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var checkCmd = new SqlCommand(
                "SELECT COUNT(1) FROM Certificates WHERE RegistrationId = @RegId", conn);
            checkCmd.Parameters.AddWithValue("@RegId", registrationId);
            if ((int)checkCmd.ExecuteScalar()! > 0) return true;

            string certNumber = $"CERT-{DateTime.Now.Year}-{registrationId:D6}";
            using var insertCmd = new SqlCommand(@"
                INSERT INTO Certificates (RegistrationId, CertificateNumber, IssueDate, Status)
                VALUES (@RegId, @CertNo, GETDATE(), 'Issued')", conn);
            insertCmd.Parameters.AddWithValue("@RegId", registrationId);
            insertCmd.Parameters.AddWithValue("@CertNo", certNumber);
            return insertCmd.ExecuteNonQuery() > 0;
        }

        private static Certificate MapCertificate(SqlDataReader r)
        {
            return new Certificate
            {
                CertificateId = r.GetInt32(r.GetOrdinal("CertificateId")),
                RegistrationId = r.GetInt32(r.GetOrdinal("RegistrationId")),
                CertificateNumber = r.GetString(r.GetOrdinal("CertificateNumber")),
                IssueDate = r.GetDateTime(r.GetOrdinal("IssueDate")),
                Status = r.GetString(r.GetOrdinal("Status")),
                StudentName = r.IsDBNull(r.GetOrdinal("StudentName")) ? null : r.GetString(r.GetOrdinal("StudentName")),
                EventName = r.IsDBNull(r.GetOrdinal("EventName")) ? null : r.GetString(r.GetOrdinal("EventName")),
                EventDate = r.IsDBNull(r.GetOrdinal("EventDate")) ? null : r.GetDateTime(r.GetOrdinal("EventDate")),
                AttendanceStatus = r.IsDBNull(r.GetOrdinal("AttendanceStatus")) ? null : r.GetString(r.GetOrdinal("AttendanceStatus")),
                RegistrationCode = r.IsDBNull(r.GetOrdinal("RegistrationCode")) ? null : r.GetString(r.GetOrdinal("RegistrationCode"))
            };
        }
    }
}
