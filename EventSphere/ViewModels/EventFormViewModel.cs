using System.ComponentModel.DataAnnotations;
using EventSphere.Models;

namespace EventSphere.ViewModels
{
    public class EventFormViewModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event name is required.")]
        [StringLength(200)]
        public string EventName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Venue is required.")]
        [StringLength(200)]
        public string Venue { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required.")]
        public DateTime EventDate { get; set; } = DateTime.Today.AddDays(7);

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        [StringLength(200)]
        public string? Organizer { get; set; }

        [Range(1, 10000)]
        public int MaximumParticipants { get; set; } = 100;

        public DateTime? RegistrationDeadline { get; set; }

        [StringLength(500)]
        public string? ImagePath { get; set; }

        public string Status { get; set; } = "Draft";

        // For dropdown
        public List<EventCategory> Categories { get; set; } = new();

        public int? CoordinatorId { get; set; }
    }
}
