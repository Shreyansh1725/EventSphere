using Microsoft.Data.SqlClient;

namespace EventSphere.Data
{
    /// <summary>
    /// Factory for creating SQL Server database connections.
    /// Demonstrates: Dependency Injection, Connection Object (ADO.NET).
    /// </summary>
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        /// <summary>
        /// Creates and returns a new open SqlConnection.
        /// Caller is responsible for disposing.
        /// </summary>
        public SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
