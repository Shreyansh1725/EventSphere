using EventSphere.Data;
using EventSphere.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EventSphere.Repositories
{
    public class VolunteerRepository : IVolunteerRepository
    {
        private readonly DbConnectionFactory _factory;

        public VolunteerRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<Volunteer> GetByEventId(int eventId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT v.*, u.FullName AS StudentName, u.Email, u.EnrollmentNumber,
                    e.EventName, e.EventDate
                FROM Volunteers v
                JOIN Users u ON v.UserId = u.UserId
                JOIN Events e ON v.EventId = e.EventId
                WHERE v.EventId = @EventId AND v.Status = 'Active'
                ORDER BY u.FullName", conn);
            cmd.Parameters.AddWithValue("@EventId", eventId);

            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);

            var list = new List<Volunteer>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Volunteer
                {
                    VolunteerId = (int)row["VolunteerId"],
                    UserId = (int)row["UserId"],
                    EventId = (int)row["EventId"],
                    AssignedAt = (DateTime)row["AssignedAt"],
                    Status = row["Status"].ToString()!,
                    StudentName = row["StudentName"].ToString(),
                    Email = row["Email"].ToString(),
                    EnrollmentNumber = row["EnrollmentNumber"] == DBNull.Value ? null : row["EnrollmentNumber"].ToString(),
                    EventName = row["EventName"].ToString(),
                    EventDate = row["EventDate"] == DBNull.Value ? null : (DateTime?)row["EventDate"]
                });
            }
            return list;
        }

        public bool Assign(int userId, int eventId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO Volunteers (UserId, EventId, AssignedAt, Status)
                VALUES (@UserId, @EventId, GETDATE(), 'Active')", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@EventId", eventId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Remove(int volunteerId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "UPDATE Volunteers SET Status = 'Removed' WHERE VolunteerId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", volunteerId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool IsAlreadyVolunteer(int userId, int eventId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(1) FROM Volunteers
                WHERE UserId = @UserId AND EventId = @EventId AND Status = 'Active'", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@EventId", eventId);
            return (int)cmd.ExecuteScalar()! > 0;
        }
    }
}
