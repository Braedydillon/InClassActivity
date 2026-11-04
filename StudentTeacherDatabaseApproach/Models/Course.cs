using System;
using System.Collections.Generic;

namespace StudentTeacherDatabaseApproach.Models;

// A course belongs to one teacher and category and can have many student enrolments.
public partial class Course
{
    public int CourseId { get; set; }

    public int TeacherProfileId { get; set; }

    public int CourseCategoryId { get; set; }

    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? CourseImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual CourseCategory CourseCategory { get; set; } = null!;

    public virtual ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    public virtual TeacherProfile TeacherProfile { get; set; } = null!;
}
