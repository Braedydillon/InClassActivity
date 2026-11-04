using System.ComponentModel.DataAnnotations;

namespace StudentTeacherDatabaseApproach.DTOs
{
    // Defines the credentials accepted by the login endpoint, with input validation attributes.
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}