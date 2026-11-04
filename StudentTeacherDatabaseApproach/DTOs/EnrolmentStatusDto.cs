using System.ComponentModel.DataAnnotations;

namespace StudentTeacherDatabaseApproach.DTOs
{
    // Carries a requested status; the controller checks allowed values and transition restrictions.
    public class EnrolmentStatusDto
    {
        [Required]
        public string Status { get; set; } = null!;
    }
}
