using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudentTeacherDatabaseApproach.Controllers;
using StudentTeacherDatabaseApproach.DTOs;

namespace Projecttests;

[TestClass]
public class CourseTests
{
    [TestMethod]
    public async Task CreateCourse_ValidCategory_AssignsCurrentTeacher()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var category = TestData.AddCategory(db);
        var controller = TestData.AsUser(new CoursesController(db), teacher);

        var action = await controller.CreateCourse(new CourseDto
        {
            CourseCategoryId = category.CourseCategoryId,
            Code = "WEB101", Title = "Web Development"
        });

        Assert.AreEqual(201, ((ObjectResult)action).StatusCode);
        var course = await db.Courses.SingleAsync();
        Assert.AreEqual(teacher.TeacherProfile!.TeacherProfileId, course.TeacherProfileId);
        Assert.AreEqual("WEB101", course.Code);
    }

    [TestMethod]
    public async Task CreateCourse_UnknownCategory_ReturnsBadRequest()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var controller = TestData.AsUser(new CoursesController(db), teacher);

        var action = await controller.CreateCourse(new CourseDto
        {
            CourseCategoryId = 999, Code = "WEB101", Title = "Web Development"
        });

        Assert.IsInstanceOfType(action, typeof(BadRequestObjectResult));
        Assert.AreEqual(0, await db.Courses.CountAsync());
    }

    [TestMethod]
    public async Task CreateCourse_DuplicateCode_ReturnsConflict()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        TestData.AddCourse(db, teacher, "WEB101");
        var category = TestData.AddCategory(db, "Another category");
        var controller = TestData.AsUser(new CoursesController(db), teacher);

        var action = await controller.CreateCourse(new CourseDto
        {
            CourseCategoryId = category.CourseCategoryId,
            Code = "WEB101", Title = "Another course"
        });

        Assert.IsInstanceOfType(action, typeof(ConflictObjectResult));
    }

    [TestMethod]
    public async Task UpdateCourse_NonOwnerTeacher_ReturnsForbid()
    {
        using var db = TestData.NewContext();
        var owner = TestData.AddTeacher(db, "T101");
        var otherTeacher = TestData.AddTeacher(db, "T102");
        var course = TestData.AddCourse(db, owner);
        var controller = TestData.AsUser(new CoursesController(db), otherTeacher);

        var action = await controller.UpdateCourse(course.CourseId, new CourseDto
        {
            CourseCategoryId = course.CourseCategoryId,
            Code = course.Code, Title = "Changed title"
        });

        Assert.IsInstanceOfType(action, typeof(ForbidResult));
        Assert.AreEqual("Introduction to Programming", (await db.Courses.SingleAsync()).Title);
    }

    [TestMethod]
    public async Task DeleteCourse_WithEnrolments_ReturnsConflict()
    {
        using var db = TestData.NewContext();
        var owner = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, owner);
        TestData.AddEnrolment(db, student, course);
        var controller = TestData.AsUser(new CoursesController(db), owner);

        var action = await controller.DeleteCourse(course.CourseId);

        Assert.IsInstanceOfType(action, typeof(ConflictObjectResult));
        Assert.AreEqual(1, await db.Courses.CountAsync());
    }

    [TestMethod]
    public async Task GetCourse_UnknownId_ReturnsNotFound()
    {
        using var db = TestData.NewContext();
        var action = await new CoursesController(db).GetCourse(999);
        Assert.IsInstanceOfType(action, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task UpdateCourse_Owner_UpdatesTitleAndDescription()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var course = TestData.AddCourse(db, teacher);
        var controller = TestData.AsUser(new CoursesController(db), teacher);
        var action = await controller.UpdateCourse(course.CourseId, new CourseDto
        {
            CourseCategoryId = course.CourseCategoryId, Code = course.Code,
            Title = "Updated title", Description = "Updated description"
        });

        Assert.IsInstanceOfType(action, typeof(OkObjectResult));
        Assert.AreEqual("Updated title", (await db.Courses.SingleAsync()).Title);
    }

    [TestMethod]
    public async Task DeleteCourse_WithoutEnrolments_RemovesCourse()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var course = TestData.AddCourse(db, teacher);
        var controller = TestData.AsUser(new CoursesController(db), teacher);

        var action = await controller.DeleteCourse(course.CourseId);

        Assert.IsInstanceOfType(action, typeof(NoContentResult));
        Assert.AreEqual(0, await db.Courses.CountAsync());
    }

    [TestMethod]
    public async Task GetMyCourses_ReturnsOnlyTeachersCourses()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db, "T101");
        var otherTeacher = TestData.AddTeacher(db, "T102");
        TestData.AddCourse(db, teacher, "WEB101");
        TestData.AddCourse(db, otherTeacher, "WEB102");
        var controller = TestData.AsUser(new CoursesController(db), teacher);

        var action = await controller.GetMyCourses();

        Assert.IsInstanceOfType(action, typeof(OkObjectResult));
        Assert.AreEqual(1, ((System.Collections.IEnumerable)((OkObjectResult)action).Value!).Cast<object>().Count());
    }

    [TestMethod]
    public async Task DeleteCourse_ByNonOwnerTeacher_ReturnsForbid()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db, "T101");
        var otherTeacher = TestData.AddTeacher(db, "T102");
        var course = TestData.AddCourse(db, teacher);
        var controller = TestData.AsUser(new CoursesController(db), otherTeacher);

        var action = await controller.DeleteCourse(course.CourseId);

        Assert.IsInstanceOfType(action, typeof(ForbidResult));
        Assert.AreEqual(1, await db.Courses.CountAsync());
    }
}
