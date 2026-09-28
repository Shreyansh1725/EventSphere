using System.ComponentModel.DataAnnotations;

namespace EventSphere.Models
{
    /// <summary>
    /// Represents a user in the system (Student or Admin).
    /// Demonstrates: Class, Properties, Encapsulation.
    /// </summary>
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? EnrollmentNumber { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        public int? Semester { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Student"; // "Student" or "Admin"

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
