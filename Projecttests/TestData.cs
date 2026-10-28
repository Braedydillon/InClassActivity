using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentTeacherDatabaseApproach.Data;
using StudentTeacherDatabaseApproach.Models;
using System.Security.Claims;

namespace Projecttests;

/// <summary>
/// Fresh in-memory EF Core database for each test. This does not touch SQL Server.
/// </summary>
internal static class TestData
{
    public static StudentTeacherDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<StudentTeacherDbContext>()
            .UseInMemoryDatabase($"tests-{Guid.NewGuid():N}")
            .Options;
        return new StudentTeacherDbContext(options);
    }

    public static User AddStudent(StudentTeacherDbContext db, string number = "S100")
    {
        var student = new User
        {
            Email = $"student-{Guid.NewGuid():N}@example.com",
            PasswordHash = "not-used-by-these-tests",
            Role = "Student",
            StudentProfile = new StudentProfile
            {
                StudentNumber = number,
                FirstName = "Alex",
                LastName = "Student"
            }
        };
        db.Users.Add(student);
        db.SaveChanges();
        return student;
    }

    public static User AddTeacher(StudentTeacherDbContext db, string number = "T100")
    {
        var teacher = new User
        {
            Email = $"teacher-{Guid.NewGuid():N}@example.com",
            PasswordHash = "not-used-by-these-tests",
            Role = "Teacher",
            TeacherProfile = new TeacherProfile
            {
                EmployeeNumber = number,
                FirstName = "Taylor",
                LastName = "Teacher"
            }
        };
        db.Users.Add(teacher);
        db.SaveChanges();
        return teacher;
    }

    public static CourseCategory AddCategory(StudentTeacherDbContext db, string name = "Programming")
    {
        var category = new CourseCategory { Name = name, Description = "Example category" };
        db.CourseCategories.Add(category);
        db.SaveChanges();
        return category;
    }

    public static Course AddCourse(StudentTeacherDbContext db, User teacher, string code = "CS101")
    {
        var category = AddCategory(db, $"Category-{Guid.NewGuid():N}");
        var course = new Course
        {
            TeacherProfileId = teacher.TeacherProfile!.TeacherProfileId,
            CourseCategoryId = category.CourseCategoryId,
            Code = code,
            Title = "Introduction to Programming",
            Description = "Test course",
            CreatedAt = DateTime.UtcNow
        };
        db.Courses.Add(course);
        db.SaveChanges();
        return course;
    }

    public static Enrolment AddEnrolment(StudentTeacherDbContext db, User student,
        Course course, string status = "Confirmed")
    {
        var enrolment = new Enrolment
        {
            StudentProfileId = student.StudentProfile!.StudentProfileId,
            CourseId = course.CourseId,
            EnrolledAt = DateTime.UtcNow,
            Status = status
        };
        db.Enrolments.Add(enrolment);
        db.SaveChanges();
        return enrolment;
    }

    public static CourseResult AddResult(StudentTeacherDbContext db, Enrolment enrolment,
        decimal mark = 80m)
    {
        var result = new CourseResult
        {
            EnrolmentId = enrolment.EnrolmentId,
            FinalMark = mark,
            Outcome = mark >= 50m ? "Pass" : "Fail",
            RecordedAt = DateTime.UtcNow
        };
        db.CourseResults.Add(result);
        db.SaveChanges();
        return result;
    }

    /// <summary>
    /// Supplies the user ID/role claims needed by controller ownership checks.
    /// This does NOT execute the ASP.NET Core [Authorize] middleware.
    /// </summary>
    public static T AsUser<T>(T controller, User user) where T : ControllerBase
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };
        return controller;
    }
}


/// <summary>
/// Replaces cookie middleware in the successful-login controller test.
/// Verifies that the controller requests a sign-in without writing a real cookie.
/// </summary>
internal sealed class RecordingAuthenticationService : Microsoft.AspNetCore.Authentication.IAuthenticationService
{
    public ClaimsPrincipal? SignedInPrincipal { get; private set; }
    public string? SignInScheme { get; private set; }

    public Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> AuthenticateAsync(
        HttpContext context, string? scheme)
        => Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.NoResult());

    public Task ChallengeAsync(HttpContext context, string? scheme,
        Microsoft.AspNetCore.Authentication.AuthenticationProperties? properties)
        => Task.CompletedTask;

    public Task ForbidAsync(HttpContext context, string? scheme,
        Microsoft.AspNetCore.Authentication.AuthenticationProperties? properties)
        => Task.CompletedTask;

    public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal,
        Microsoft.AspNetCore.Authentication.AuthenticationProperties? properties)
    {
        SignInScheme = scheme;
        SignedInPrincipal = principal;
        return Task.CompletedTask;
    }

    public Task SignOutAsync(HttpContext context, string? scheme,
        Microsoft.AspNetCore.Authentication.AuthenticationProperties? properties)
        => Task.CompletedTask;
}
