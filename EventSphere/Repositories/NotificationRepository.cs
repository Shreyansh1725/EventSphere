using EventSphere.Data;
using EventSphere.Models;
using Microsoft.Data.SqlClient;

namespace EventSphere.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DbConnectionFactory _factory;

        public NotificationRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<Notification> GetByUserId(int userId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT * FROM Notifications
                WHERE UserId = @UserId OR UserId IS NULL
                ORDER BY CreatedAt DESC", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = cmd.ExecuteReader();
            var list = new List<Notification>();
            while (reader.Read())
                list.Add(MapNotification(reader));
            return list;
        }

        public int GetUnreadCountByUserId(int userId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(1) FROM Notifications
                WHERE (UserId = @UserId OR UserId IS NULL) AND IsRead = 0", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            return (int)cmd.ExecuteScalar()!;
        }

        public bool MarkAllRead(int userId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                UPDATE Notifications SET IsRead = 1
                WHERE UserId = @UserId OR UserId IS NULL", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            return cmd.ExecuteNonQuery() >= 0;
        }

        public bool Create(Notification notification)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO Notifications (UserId, Title, Message, IsRead, CreatedAt)
                VALUES (@UserId, @Title, @Msg, 0, GETDATE())", conn);
            cmd.Parameters.AddWithValue("@UserId", (object?)notification.UserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Title", notification.Title);
            cmd.Parameters.AddWithValue("@Msg", notification.Message);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool BroadcastToAllStudents(string title, string message)
        {
            return Create(new Notification { UserId = null, Title = title, Message = message });
        }

        public List<Notification> GetAll()
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT * FROM Notifications ORDER BY CreatedAt DESC", conn);
            using var reader = cmd.ExecuteReader();
            var list = new List<Notification>();
            while (reader.Read())
                list.Add(MapNotification(reader));
            return list;
        }

        private static Notification MapNotification(SqlDataReader r)
        {
            return new Notification
            {
                NotificationId = r.GetInt32(r.GetOrdinal("NotificationId")),
                UserId = r.IsDBNull(r.GetOrdinal("UserId")) ? null : r.GetInt32(r.GetOrdinal("UserId")),
                Title = r.GetString(r.GetOrdinal("Title")),
                Message = r.GetString(r.GetOrdinal("Message")),
                IsRead = r.GetBoolean(r.GetOrdinal("IsRead")),
                CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt"))
            };
        }
    }
}
