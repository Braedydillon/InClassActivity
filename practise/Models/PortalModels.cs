using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace practise.Models;

public class LoginForm {
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
}
public class RegisterForm : LoginForm, IValidatableObject {
    [Required, StringLength(100)] public string FirstName { get; set; } = "";
    [Required, StringLength(100)] public string LastName { get; set; } = "";
    [Required, RegularExpression("Student|Teacher")] public string Role { get; set; } = "Student";
    [StringLength(30)] public string? StudentNumber { get; set; }
    [StringLength(30)] public string? EmployeeNumber { get; set; }
    [Required, Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
    public IEnumerable<ValidationResult> Validate(ValidationContext context) {
        if (Role == "Student" && string.IsNullOrWhiteSpace(StudentNumber)) yield return new("Enter your student number.", [nameof(StudentNumber)]);
        if (Role == "Teacher" && string.IsNullOrWhiteSpace(EmployeeNumber)) yield return new("Enter your employee number.", [nameof(EmployeeNumber)]);
    }
}
public class ProfileForm {
    [Required, StringLength(100)] public string FirstName { get; set; } = "";
    [Required, StringLength(100)] public string LastName { get; set; } = "";
}
public class AccountProfile {
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public ProfileDetails Profile { get; set; } = new();
}
public class ProfileDetails : ProfileForm {
    public int TeacherProfileId { get; set; }
    public string? StudentNumber { get; set; }
    public string? EmployeeNumber { get; set; }
}
public class CourseForm {
    public int CourseId { get; set; }
    [Required, StringLength(30)] public string Code { get; set; } = "";
    [Required, StringLength(150)] public string Title { get; set; } = "";
    [StringLength(1000)] public string? Description { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Choose a category.")] public int CourseCategoryId { get; set; }
    [ValidateNever] public List<Category> Categories { get; set; } = [];
}
public class Course : CourseForm {
    public string CategoryName { get; set; } = "";
    public string? TeacherFirstName { get; set; }
    public string? TeacherLastName { get; set; }
    public int TeacherProfileId { get; set; }
}
public class Category {
    public int CourseCategoryId { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [StringLength(500)] public string? Description { get; set; }
}
public class Enrolment {
    public int EnrolmentId { get; set; }
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = "";
    public string CourseTitle { get; set; } = "";
    public string Status { get; set; } = "";
    public string StudentNumber { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}
public class CourseResult {
    public int CourseResultId { get; set; }
    public int EnrolmentId { get; set; }
    public string CourseCode { get; set; } = "";
    public string CourseTitle { get; set; } = "";
    public decimal FinalMark { get; set; }
    public string Outcome { get; set; } = "";
}
public class CourseDetails {
    public Course Course { get; set; } = new();
    public bool CanManage { get; set; }
    public bool IsEnrolled { get; set; }
    public List<Enrolment> Enrolments { get; set; } = [];
    public List<CourseResult> Results { get; set; } = [];
}
public class LearningPage {
    public List<Enrolment> Enrolments { get; set; } = [];
    public List<CourseResult> Results { get; set; } = [];
}
