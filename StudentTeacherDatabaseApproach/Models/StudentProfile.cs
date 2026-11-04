using System;
using System.Collections.Generic;

namespace StudentTeacherDatabaseApproach.Models;

// Stores student-specific details and links the account to its enrolments.
public partial class StudentProfile
{
    public int StudentProfileId { get; set; }

    public int UserId { get; set; }

    public string StudentNumber { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? ProfileImageUrl { get; set; }

    public virtual ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    public virtual User User { get; set; } = null!;
}
