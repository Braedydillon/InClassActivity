using System;
using System.Collections.Generic;

namespace StudentTeacherDatabaseApproach.Models;

// Links a student to a course; its result is absent until a mark is recorded.
public partial class Enrolment
{
    public int EnrolmentId { get; set; }

    public int StudentProfileId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrolledAt { get; set; }

    public string Status { get; set; } = null!;

    public virtual Course Course { get; set; } = null!;

    public virtual CourseResult? CourseResult { get; set; }

    public virtual StudentProfile StudentProfile { get; set; } = null!;
}
