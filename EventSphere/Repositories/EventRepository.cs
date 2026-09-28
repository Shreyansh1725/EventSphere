using EventSphere.Data;
using EventSphere.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EventSphere.Repositories
{
    /// <summary>
    /// ADO.NET implementation for Event CRUD operations.
    /// Demonstrates: SqlDataAdapter, DataTable, DataSet, CRUD.
    /// </summary>
    public class EventRepository : IEventRepository
    {
        private readonly DbConnectionFactory _factory;

        public EventRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public List<Event> GetAll(string? search = null, int? categoryId = null,
            string? status = null, string? dateFilter = null)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();

            var sql = @"
                SELECT e.*, c.CategoryName
                FROM Events e
                LEFT JOIN EventCategories c ON e.CategoryId = c.CategoryId
                WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(search))
                sql += " AND (e.EventName LIKE @Search OR e.Venue LIKE @Search OR e.Organizer LIKE @Search)";
            if (categoryId.HasValue)
                sql += " AND e.CategoryId = @CategoryId";
            if (!string.IsNullOrWhiteSpace(status))
                sql += " AND e.Status = @Status";
            if (dateFilter == "upcoming")
                sql += " AND e.EventDate >= CAST(GETDATE() AS DATE)";
            else if (dateFilter == "past")
                sql += " AND e.EventDate < CAST(GETDATE() AS DATE)";

            sql += " ORDER BY e.EventDate DESC";

            using var cmd = new SqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(search))
                cmd.Parameters.AddWithValue("@Search", $"%{search}%");
            if (categoryId.HasValue)
                cmd.Parameters.AddWithValue("@CategoryId", categoryId.Value);
            if (!string.IsNullOrWhiteSpace(status))
                cmd.Parameters.AddWithValue("@Status", status);

            // Demonstrating DataAdapter and DataTable usage
            using var adapter = new SqlDataAdapter(cmd);
            var ds = new DataSet();
            adapter.Fill(ds, "Events");

            var events = new List<Event>();
            foreach (DataRow row in ds.Tables["Events"]!.Rows)
                events.Add(MapEventFromRow(row));
            return events;
        }

        public Event? GetById(int eventId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT e.*, c.CategoryName
                FROM Events e
                LEFT JOIN EventCategories c ON e.CategoryId = c.CategoryId
                WHERE e.EventId = @EventId", conn);
            cmd.Parameters.AddWithValue("@EventId", eventId);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MapEvent(reader);
            return null;
        }

        public int Create(Event evt)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                INSERT INTO Events (EventName, Description, CategoryId, Venue, EventDate,
                    StartTime, EndTime, Organizer, MaximumParticipants, AvailableSeats,
                    RegistrationDeadline, ImagePath, Status, CreatedAt, CoordinatorId)
                OUTPUT INSERTED.EventId
                VALUES (@Name, @Desc, @CatId, @Venue, @Date,
                    @Start, @End, @Org, @Max, @Avail,
                    @Deadline, @Img, @Status, GETDATE(), @CoordinatorId)", conn);
            cmd.Parameters.AddWithValue("@Name", evt.EventName);
            cmd.Parameters.AddWithValue("@Desc", (object?)evt.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CatId", evt.CategoryId);
            cmd.Parameters.AddWithValue("@Venue", evt.Venue);
            cmd.Parameters.AddWithValue("@Date", evt.EventDate);
            cmd.Parameters.AddWithValue("@Start", (object?)evt.StartTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@End", (object?)evt.EndTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Org", (object?)evt.Organizer ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Max", evt.MaximumParticipants);
            cmd.Parameters.AddWithValue("@Avail", evt.MaximumParticipants);
            cmd.Parameters.AddWithValue("@Deadline", (object?)evt.RegistrationDeadline ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Img", (object?)evt.ImagePath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", evt.Status);
            cmd.Parameters.AddWithValue("@CoordinatorId", (object?)evt.CoordinatorId ?? DBNull.Value);
            return (int)cmd.ExecuteScalar()!;
        }

        public bool Update(Event evt)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                UPDATE Events SET EventName=@Name, Description=@Desc, CategoryId=@CatId,
                    Venue=@Venue, EventDate=@Date, StartTime=@Start, EndTime=@End,
                    Organizer=@Org, MaximumParticipants=@Max, RegistrationDeadline=@Deadline,
                    ImagePath=@Img, Status=@Status
                WHERE EventId=@EventId", conn);
            cmd.Parameters.AddWithValue("@Name", evt.EventName);
            cmd.Parameters.AddWithValue("@Desc", (object?)evt.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CatId", evt.CategoryId);
            cmd.Parameters.AddWithValue("@Venue", evt.Venue);
            cmd.Parameters.AddWithValue("@Date", evt.EventDate);
            cmd.Parameters.AddWithValue("@Start", (object?)evt.StartTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@End", (object?)evt.EndTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Org", (object?)evt.Organizer ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Max", evt.MaximumParticipants);
            cmd.Parameters.AddWithValue("@Deadline", (object?)evt.RegistrationDeadline ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Img", (object?)evt.ImagePath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", evt.Status);
            cmd.Parameters.AddWithValue("@EventId", evt.EventId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int eventId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand("DELETE FROM Events WHERE EventId = @EventId", conn);
            cmd.Parameters.AddWithValue("@EventId", eventId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateStatus(int eventId, string status)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "UPDATE Events SET Status = @Status WHERE EventId = @EventId", conn);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@EventId", eventId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DecrementSeats(int eventId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                UPDATE Events SET AvailableSeats = AvailableSeats - 1
                WHERE EventId = @EventId AND AvailableSeats > 0", conn);
            cmd.Parameters.AddWithValue("@EventId", eventId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool IncrementSeats(int eventId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                UPDATE Events SET AvailableSeats = AvailableSeats + 1
                WHERE EventId = @EventId", conn);
            cmd.Parameters.AddWithValue("@EventId", eventId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<EventCategory> GetCategories()
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(
                "SELECT * FROM EventCategories WHERE IsActive = 1 ORDER BY CategoryName", conn);
            using var reader = cmd.ExecuteReader();
            var categories = new List<EventCategory>();
            while (reader.Read())
            {
                categories.Add(new EventCategory
                {
                    CategoryId = reader.GetInt32(0),
                    CategoryName = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                    IsActive = reader.GetBoolean(3)
                });
            }
            return categories;
        }

        public List<Event> GetUpcoming(int count = 5)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand($@"
                SELECT TOP {count} e.*, c.CategoryName
                FROM Events e
                LEFT JOIN EventCategories c ON e.CategoryId = c.CategoryId
                WHERE e.Status = 'Published' AND e.EventDate >= CAST(GETDATE() AS DATE)
                ORDER BY e.EventDate ASC", conn);
            using var reader = cmd.ExecuteReader();
            var events = new List<Event>();
            while (reader.Read())
                events.Add(MapEvent(reader));
            return events;
        }

        public List<Event> GetByCoordinatorId(int coordinatorId)
        {
            using var conn = _factory.CreateConnection();
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT e.*, c.CategoryName
                FROM Events e
                LEFT JOIN EventCategories c ON e.CategoryId = c.CategoryId
                WHERE e.CoordinatorId = @CoordinatorId
                ORDER BY e.EventDate DESC", conn);
            cmd.Parameters.AddWithValue("@CoordinatorId", coordinatorId);
            using var reader = cmd.ExecuteReader();
            var events = new List<Event>();
            while (reader.Read())
                events.Add(MapEvent(reader));
            return events;
        }

        private static Event MapEvent(SqlDataReader reader)
        {
            return new Event
            {
                EventId = reader.GetInt32(reader.GetOrdinal("EventId")),
                EventName = reader.GetString(reader.GetOrdinal("EventName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? null : reader.GetString(reader.GetOrdinal("CategoryName")),
                Venue = reader.GetString(reader.GetOrdinal("Venue")),
                EventDate = reader.GetDateTime(reader.GetOrdinal("EventDate")),
                StartTime = reader.IsDBNull(reader.GetOrdinal("StartTime")) ? null : reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
                EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetTimeSpan(reader.GetOrdinal("EndTime")),
                Organizer = reader.IsDBNull(reader.GetOrdinal("Organizer")) ? null : reader.GetString(reader.GetOrdinal("Organizer")),
                MaximumParticipants = reader.GetInt32(reader.GetOrdinal("MaximumParticipants")),
                AvailableSeats = reader.GetInt32(reader.GetOrdinal("AvailableSeats")),
                RegistrationDeadline = reader.IsDBNull(reader.GetOrdinal("RegistrationDeadline")) ? null : reader.GetDateTime(reader.GetOrdinal("RegistrationDeadline")),
                ImagePath = reader.IsDBNull(reader.GetOrdinal("ImagePath")) ? null : reader.GetString(reader.GetOrdinal("ImagePath")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CoordinatorId = reader.IsDBNull(reader.GetOrdinal("CoordinatorId")) ? null : reader.GetInt32(reader.GetOrdinal("CoordinatorId"))
            };
        }

        private static Event MapEventFromRow(DataRow row)
        {
            return new Event
            {
                EventId = (int)row["EventId"],
                EventName = row["EventName"].ToString()!,
                Description = row["Description"] == DBNull.Value ? null : row["Description"].ToString(),
                CategoryId = (int)row["CategoryId"],
                CategoryName = row["CategoryName"] == DBNull.Value ? null : row["CategoryName"].ToString(),
                Venue = row["Venue"].ToString()!,
                EventDate = (DateTime)row["EventDate"],
                StartTime = row["StartTime"] == DBNull.Value ? null : (TimeSpan?)row["StartTime"],
                EndTime = row["EndTime"] == DBNull.Value ? null : (TimeSpan?)row["EndTime"],
                Organizer = row["Organizer"] == DBNull.Value ? null : row["Organizer"].ToString(),
                MaximumParticipants = (int)row["MaximumParticipants"],
                AvailableSeats = (int)row["AvailableSeats"],
                RegistrationDeadline = row["RegistrationDeadline"] == DBNull.Value ? null : (DateTime?)row["RegistrationDeadline"],
                ImagePath = row["ImagePath"] == DBNull.Value ? null : row["ImagePath"].ToString(),
                Status = row["Status"].ToString()!,
                CreatedAt = (DateTime)row["CreatedAt"],
                CoordinatorId = row.Table.Columns.Contains("CoordinatorId") && row["CoordinatorId"] != DBNull.Value ? (int)row["CoordinatorId"] : null
            };
        }
    }
}
