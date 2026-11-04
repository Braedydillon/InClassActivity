using System.ComponentModel.DataAnnotations;

namespace StudentTeacherDatabaseApproach.DTOs
{
    // Accepts a mark from zero to one hundred; the server calculates the pass or fail outcome.
    public class CourseResultDto
    {
        [Range(typeof(decimal), "0", "100")]
        public decimal FinalMark { get; set; }
    }
}
