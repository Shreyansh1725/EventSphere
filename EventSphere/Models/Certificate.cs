namespace EventSphere.Models
{
    /// <summary>
    /// Represents a participation certificate.
    /// </summary>
    public class Certificate
    {
        public int CertificateId { get; set; }
        public int RegistrationId { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Issued";

        // Populated by JOINs
        public string? StudentName { get; set; }
        public string? EventName { get; set; }
        public DateTime? EventDate { get; set; }
        public string? AttendanceStatus { get; set; }
        public string? RegistrationCode { get; set; }
    }
}
