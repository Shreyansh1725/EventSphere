using System.ComponentModel.DataAnnotations;

namespace EventSphere.Models
{
    /// <summary>
    /// Represents a college event.
    /// Demonstrates: Class, Properties, Data Annotations.
    /// </summary>
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        [StringLength(200)]
        public string EventName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        [Required]
        [StringLength(200)]
        public string Venue { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        [StringLength(200)]
        public string? Organizer { get; set; }

        [Range(1, 10000)]
        public int MaximumParticipants { get; set; } = 100;

        public int AvailableSeats { get; set; } = 100;

        public DateTime? RegistrationDeadline { get; set; }

        [StringLength(500)]
        public string? ImagePath { get; set; }

        // Status: Draft, Published, Cancelled, Completed
        public string Status { get; set; } = "Draft";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? CoordinatorId { get; set; }
    }
}
