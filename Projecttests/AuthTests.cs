using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudentTeacherDatabaseApproach.DTOs;
using StudentTeacherDatabaseApproach.Models;

namespace Projecttests;

[TestClass]
public class AuthTests
{
    private static RegisterDto StudentDto() => new()
    {
        Email = "new.student@example.com",
        Password = "Password123!",
        Role = "Student",
        FirstName = "Sam",
        LastName = "Smith",
        StudentNumber = "S200"
    };

    [TestMethod]
    public async Task Register_Student_CreatesUserAndStudentProfileWithHashedPassword()
    {
        using var db = TestData.NewContext();
        var action = await new AuthController(db).Register(StudentDto());

        Assert.IsInstanceOfType(action, typeof(ObjectResult));
        Assert.AreEqual(201, ((ObjectResult)action).StatusCode);
        var user = await db.Users.Include(u => u.StudentProfile).SingleAsync();
        Assert.AreEqual("Student", user.Role);
        Assert.AreEqual("S200", user.StudentProfile!.StudentNumber);
        Assert.AreNotEqual("Password123!", user.PasswordHash);
        Assert.AreNotEqual(PasswordVerificationResult.Failed,
            new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, "Password123!"));
    }

    [TestMethod]
    public async Task Register_Teacher_CreatesTeacherProfile()
    {
        using var db = TestData.NewContext();
        var dto = StudentDto();
        dto.Role = "Teacher";
        dto.StudentNumber = null;
        dto.EmployeeNumber = "EMP200";

        var action = await new AuthController(db).Register(dto);

        Assert.AreEqual(201, ((ObjectResult)action).StatusCode);
        var user = await db.Users.Include(u => u.TeacherProfile).SingleAsync();
        Assert.AreEqual("EMP200", user.TeacherProfile!.EmployeeNumber);
        Assert.AreEqual("Teacher", user.Role);
    }

    [TestMethod]
    public async Task Register_InvalidRole_ReturnsBadRequest()
    {
        using var db = TestData.NewContext();
        var dto = StudentDto();
        dto.Role = "Admin";

        var action = await new AuthController(db).Register(dto);

        Assert.IsInstanceOfType(action, typeof(BadRequestObjectResult));
        Assert.AreEqual(0, await db.Users.CountAsync());
    }

    [TestMethod]
    public async Task Register_WithoutStudentNumber_ReturnsBadRequest()
    {
        using var db = TestData.NewContext();
        var dto = StudentDto();
        dto.StudentNumber = null;

        var action = await new AuthController(db).Register(dto);

        Assert.IsInstanceOfType(action, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        using var db = TestData.NewContext();
        var existing = TestData.AddStudent(db);
        var dto = StudentDto();
        dto.Email = existing.Email;

        var action = await new AuthController(db).Register(dto);

        Assert.IsInstanceOfType(action, typeof(ConflictObjectResult));
        Assert.AreEqual(1, await db.Users.CountAsync());
    }

    [TestMethod]
    public async Task Register_DuplicateStudentNumber_ReturnsConflict()
    {
        using var db = TestData.NewContext();
        TestData.AddStudent(db, "S200");

        var action = await new AuthController(db).Register(StudentDto());

        Assert.IsInstanceOfType(action, typeof(ConflictObjectResult));
    }

    [TestMethod]
    public async Task Login_UnknownEmail_ReturnsUnauthorized()
    {
        using var db = TestData.NewContext();
        var action = await new AuthController(db).Login(new LoginDto
        {
            Email = "missing@example.com", Password = "WrongPassword"
        });
        Assert.IsInstanceOfType(action, typeof(UnauthorizedResult));
    }

    [TestMethod]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        using var db = TestData.NewContext();
        var dto = StudentDto();
        await new AuthController(db).Register(dto);

        var action = await new AuthController(db).Login(new LoginDto
        {
            Email = dto.Email, Password = "WrongPassword"
        });
        Assert.IsInstanceOfType(action, typeof(UnauthorizedResult));
    }

    [TestMethod]
    public async Task Register_TeacherWithoutEmployeeNumber_ReturnsBadRequest()
    {
        using var db = TestData.NewContext();
        var dto = StudentDto();
        dto.Role = "Teacher";
        dto.StudentNumber = null;
        dto.EmployeeNumber = null;

        var action = await new AuthController(db).Register(dto);

        Assert.IsInstanceOfType(action, typeof(BadRequestObjectResult));
        Assert.AreEqual(0, await db.Users.CountAsync());
    }

    [TestMethod]
    public async Task Register_DuplicateEmployeeNumber_ReturnsConflict()
    {
        using var db = TestData.NewContext();
        TestData.AddTeacher(db, "T200");
        var dto = StudentDto();
        dto.Role = "Teacher";
        dto.StudentNumber = null;
        dto.EmployeeNumber = "T200";

        var action = await new AuthController(db).Register(dto);

        Assert.IsInstanceOfType(action, typeof(ConflictObjectResult));
        Assert.AreEqual(1, await db.Users.CountAsync());
    }

    [TestMethod]
    public async Task Login_CorrectCredentials_SignsInWithStudentRole()
    {
        using var db = TestData.NewContext();
        var dto = StudentDto();
        await new AuthController(db).Register(dto);
        var fakeAuthentication = new RecordingAuthenticationService();
        using var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection()
            .AddSingleton<Microsoft.AspNetCore.Authentication.IAuthenticationService>(fakeAuthentication)
            .BuildServiceProvider();
        var controller = new AuthController(db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    RequestServices = services
                }
            }
        };

        var action = await controller.Login(new LoginDto
        {
            Email = dto.Email, Password = dto.Password
        });

        Assert.IsInstanceOfType(action, typeof(OkObjectResult));
        Assert.IsNotNull(fakeAuthentication.SignedInPrincipal);
        Assert.IsTrue(fakeAuthentication.SignedInPrincipal.IsInRole("Student"));
        Assert.AreEqual(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
            fakeAuthentication.SignInScheme);
    }

    [TestMethod]
    public async Task GetProfile_Student_ReturnsOwnProfile()
    {
        using var db = TestData.NewContext();
        var student = TestData.AddStudent(db);
        var controller = TestData.AsUser(new AuthController(db), student);

        var action = await controller.GetProfile();

        Assert.IsInstanceOfType(action, typeof(OkObjectResult));
        Assert.IsNotNull(((OkObjectResult)action).Value);
    }

    [TestMethod]
    public async Task UpdateProfile_Teacher_UpdatesOnlyOwnDetails()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db, "T101");
        var other = TestData.AddTeacher(db, "T102");
        var controller = TestData.AsUser(new AuthController(db), teacher);

        var action = await controller.UpdateProfile(new UpdateProfileDto
        {
            FirstName = "Updated", LastName = "Name"
        });

        Assert.IsInstanceOfType(action, typeof(OkObjectResult));
        Assert.AreEqual("Updated", (await db.TeacherProfiles.FindAsync(teacher.TeacherProfile!.TeacherProfileId))!.FirstName);
        Assert.AreEqual("Taylor", (await db.TeacherProfiles.FindAsync(other.TeacherProfile!.TeacherProfileId))!.FirstName);
    }

    [TestMethod]
    public async Task GetProfile_WithoutUserId_ReturnsUnauthorized()
    {
        using var db = TestData.NewContext();
        var controller = new AuthController(db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            }
        };

        var action = await controller.GetProfile();

        Assert.IsInstanceOfType(action, typeof(UnauthorizedResult));
    }
}
