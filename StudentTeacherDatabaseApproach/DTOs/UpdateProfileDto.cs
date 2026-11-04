using System.ComponentModel.DataAnnotations;

namespace StudentTeacherDatabaseApproach.DTOs
{
    // Request body for PUT /api/auth/profile. 
    // Limits profile changes to names so account role and identity numbers cannot be changed here.
    public class UpdateProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = null!;
    }
}
