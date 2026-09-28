using System.ComponentModel.DataAnnotations;

namespace EventSphere.Models
{
    /// <summary>
    /// Represents an event category (Technical, Cultural, etc.).
    /// </summary>
    public class EventCategory
    {
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
