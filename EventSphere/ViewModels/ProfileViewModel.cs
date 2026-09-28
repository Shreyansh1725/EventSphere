using System.ComponentModel.DataAnnotations;

namespace EventSphere.ViewModels
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EnrollmentNumber { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [Range(1, 8)]
        public int? Semester { get; set; }
    }
}
