using System;
using System.Collections.Generic;

namespace StudentTeacherDatabaseApproach.Models;

// Stores the final mark and calculated outcome for one enrolment.
public partial class CourseResult
{
    public int CourseResultId { get; set; }

    public int EnrolmentId { get; set; }

    public decimal FinalMark { get; set; }

    public string Outcome { get; set; } = null!;

    public DateTime RecordedAt { get; set; }

    public virtual Enrolment Enrolment { get; set; } = null!;
}
