using System.ComponentModel.DataAnnotations;

namespace StudentTeacherDatabaseApproach.DTOs
{
    // Accepts editable course fields; the controller derives the teacher from the signed-in account.
    public class CourseDto
    {
        [Range(1, int.MaxValue)]
        public int CourseCategoryId { get; set; }

        [Required]
        [MaxLength(30)]
        public string Code { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
