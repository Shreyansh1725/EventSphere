namespace EventSphere.Models
{
    /// <summary>
    /// Represents a notification sent to a student.
    /// </summary>
    public class Notification
    {
        public int NotificationId { get; set; }
        public int? UserId { get; set; } // null = broadcast to all students
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
