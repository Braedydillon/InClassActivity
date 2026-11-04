using System.ComponentModel.DataAnnotations;

namespace StudentTeacherDatabaseApproach.DTOs
{
    //DTO used for creating and updating course categories
    // Validates category input lengths before a create or update request is processed.
    public class CourseCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
