using EventSphere.Data;
using EventSphere.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EventSphere.Repositories
{
    /// <summary>
    /// ADO.NET implementation for user data access.
    /// Demonstrates: SqlConnection, SqlCommand, SqlDataReader, parameterized queries.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly DbConnectionFactory _factory;

        public UserRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public User? GetByEmail(string email)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT * FROM Users WHERE Email = @Email AND IsActive = 1", conn);
            cmd.Parameters.AddWithValue("@Email", email);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapUser(reader);
            return null;
        }

        public User? GetById(int userId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand("SELECT * FROM Users WHERE UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapUser(reader);
            return null;
        }

        public bool EmailExists(string email)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM Users WHERE Email = @Email", conn);
            cmd.Parameters.AddWithValue("@Email", email);
            return (int)cmd.ExecuteScalar()! > 0;
        }

        public bool EnrollmentNumberExists(string enrollmentNumber)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT COUNT(1) FROM Users WHERE EnrollmentNumber = @En", conn);
            cmd.Parameters.AddWithValue("@En", enrollmentNumber);
            return (int)cmd.ExecuteScalar()! > 0;
        }

        public int Create(User user, string password)
        {
            string hash = BCryptHashPassword(password);
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO Users (FullName, EnrollmentNumber, Email, Phone, Department, Semester,
                    PasswordHash, Role, IsActive, CreatedAt)
                OUTPUT INSERTED.UserId
                VALUES (@FullName, @En, @Email, @Phone, @Dept, @Sem,
                    @Hash, @Role, 1, GETDATE())", conn);
            cmd.Parameters.AddWithValue("@FullName", user.FullName);
            cmd.Parameters.AddWithValue("@En", (object?)user.EnrollmentNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@Phone", (object?)user.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Dept", (object?)user.Department ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Sem", (object?)user.Semester ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Hash", hash);
            cmd.Parameters.AddWithValue("@Role", user.Role);
            return (int)cmd.ExecuteScalar()!;
        }

        public bool UpdateProfile(User user)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                UPDATE Users SET Phone = @Phone, Department = @Dept, Semester = @Sem
                WHERE UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@Phone", (object?)user.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Dept", (object?)user.Department ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Sem", (object?)user.Semester ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", user.UserId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdatePassword(int userId, string newPasswordHash)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "UPDATE Users SET PasswordHash = @Hash WHERE UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@Hash", newPasswordHash);
            cmd.Parameters.AddWithValue("@UserId", userId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool ToggleActive(int userId, bool isActive)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "UPDATE Users SET IsActive = @IsActive WHERE UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@IsActive", isActive);
            cmd.Parameters.AddWithValue("@UserId", userId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<User> GetAllStudents(string? searchTerm = null, string? department = null)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            var sql = "SELECT * FROM Users WHERE Role = 'Student'";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                sql += " AND (FullName LIKE @Search OR Email LIKE @Search OR EnrollmentNumber LIKE @Search)";
            if (!string.IsNullOrWhiteSpace(department))
                sql += " AND Department = @Dept";
            sql += " ORDER BY FullName";

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(searchTerm))
                cmd.Parameters.AddWithValue("@Search", $"%{searchTerm}%");
            if (!string.IsNullOrWhiteSpace(department))
                cmd.Parameters.AddWithValue("@Dept", department);

            using var adapter = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);

            var users = new List<User>();
            foreach (DataRow row in dt.Rows)
                users.Add(MapUserFromRow(row));
            return users;
        }

        public string? GetPasswordHash(int userId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT PasswordHash FROM Users WHERE UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            return cmd.ExecuteScalar()?.ToString();
        }

        // Simple BCrypt-style hashing using SHA256 for academic demonstration
        // In production, use BCrypt.Net or similar
        private static string BCryptHashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes("EventSphere_Salt_" + password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static string HashPassword(string password) => BCryptHashPassword(password);

        public static bool VerifyPassword(string password, string hash)
        {
            return BCryptHashPassword(password) == hash;
        }

        private static User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                EnrollmentNumber = reader.IsDBNull(reader.GetOrdinal("EnrollmentNumber")) ? null : reader.GetString(reader.GetOrdinal("EnrollmentNumber")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                Department = reader.IsDBNull(reader.GetOrdinal("Department")) ? null : reader.GetString(reader.GetOrdinal("Department")),
                Semester = reader.IsDBNull(reader.GetOrdinal("Semester")) ? null : reader.GetInt32(reader.GetOrdinal("Semester")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        private static User MapUserFromRow(DataRow row)
        {
            return new User
            {
                UserId = (int)row["UserId"],
                FullName = row["FullName"].ToString()!,
                EnrollmentNumber = row["EnrollmentNumber"] == DBNull.Value ? null : row["EnrollmentNumber"].ToString(),
                Email = row["Email"].ToString()!,
                Phone = row["Phone"] == DBNull.Value ? null : row["Phone"].ToString(),
                Department = row["Department"] == DBNull.Value ? null : row["Department"].ToString(),
                Semester = row["Semester"] == DBNull.Value ? null : (int?)row["Semester"],
                PasswordHash = row["PasswordHash"].ToString()!,
                Role = row["Role"].ToString()!,
                IsActive = (bool)row["IsActive"],
                CreatedAt = (DateTime)row["CreatedAt"]
            };
        }
    }
}
