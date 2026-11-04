using System;
using System.Collections.Generic;

namespace StudentTeacherDatabaseApproach.Models;

// Groups related courses under a shared name and description.
public partial class CourseCategory
{
    public int CourseCategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
