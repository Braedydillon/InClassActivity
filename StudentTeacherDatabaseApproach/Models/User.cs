using System;
using System.Collections.Generic;

namespace StudentTeacherDatabaseApproach.Models;

// Stores login credentials and role, with a linked student or teacher profile.
public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public virtual StudentProfile? StudentProfile { get; set; }

    public virtual TeacherProfile? TeacherProfile { get; set; }
}
