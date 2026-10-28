using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudentTeacherDatabaseApproach.Controllers;
using StudentTeacherDatabaseApproach.DTOs;

namespace Projecttests;

[TestClass]
public class EnrolmentTests
{
    [TestMethod]
    public async Task Enrol_ValidStudentAndCourse_CreatesPendingEnrolment()
    {
        using var db = TestData.NewContext();
        var student = TestData.AddStudent(db);
        var teacher = TestData.AddTeacher(db);
        var course = TestData.AddCourse(db, teacher);
        var controller = TestData.AsUser(new EnrolmentsController(db), student);

        var action = await controller.Enrol(course.CourseId);

        Assert.AreEqual(201, ((ObjectResult)action).StatusCode);
        var saved = await db.Enrolments.SingleAsync();
        Assert.AreEqual("Pending", saved.Status);
        Assert.AreEqual(student.StudentProfile!.StudentProfileId, saved.StudentProfileId);
        Assert.AreEqual(course.CourseId, saved.CourseId);
    }

    [TestMethod]
    public async Task Enrol_SameCourseTwice_ReturnsConflict()
    {
        using var db = TestData.NewContext();
        var student = TestData.AddStudent(db);
        var teacher = TestData.AddTeacher(db);
        var course = TestData.AddCourse(db, teacher);
        TestData.AddEnrolment(db, student, course);
        var controller = TestData.AsUser(new EnrolmentsController(db), student);

        var action = await controller.Enrol(course.CourseId);

        Assert.IsInstanceOfType(action, typeof(ConflictObjectResult));
        Assert.AreEqual(1, await db.Enrolments.CountAsync());
    }

    [TestMethod]
    public async Task Enrol_UnknownCourse_ReturnsNotFound()
    {
        using var db = TestData.NewContext();
        var student = TestData.AddStudent(db);
        var controller = TestData.AsUser(new EnrolmentsController(db), student);

        var action = await controller.Enrol(999);

        Assert.IsInstanceOfType(action, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task GetEnrolment_OtherStudentsEnrolment_ReturnsForbid()
    {
        using var db = TestData.NewContext();
        var ownerStudent = TestData.AddStudent(db, "S101");
        var differentStudent = TestData.AddStudent(db, "S102");
        var teacher = TestData.AddTeacher(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, ownerStudent, course);
        var controller = TestData.AsUser(new EnrolmentsController(db), differentStudent);

        var action = await controller.GetEnrolment(enrolment.EnrolmentId);

        Assert.IsInstanceOfType(action, typeof(ForbidResult));
    }

    [TestMethod]
    public async Task UpdateStatus_InvalidStatus_ReturnsBadRequest()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course, "Pending");
        var controller = TestData.AsUser(new EnrolmentsController(db), teacher);

        var action = await controller.UpdateStatus(enrolment.EnrolmentId,
            new EnrolmentStatusDto { Status = "Accepted" });

        Assert.IsInstanceOfType(action, typeof(BadRequestObjectResult));
        Assert.AreEqual("Pending", (await db.Enrolments.SingleAsync()).Status);
    }

    [TestMethod]
    public async Task UpdateStatus_OwningTeacher_CanConfirmEnrolment()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course, "Pending");
        var controller = TestData.AsUser(new EnrolmentsController(db), teacher);

        var action = await controller.UpdateStatus(enrolment.EnrolmentId,
            new EnrolmentStatusDto { Status = "Confirmed" });

        Assert.IsInstanceOfType(action, typeof(OkObjectResult));
        Assert.AreEqual("Confirmed", (await db.Enrolments.SingleAsync()).Status);
    }

    [TestMethod]
    public async Task UpdateStatus_CancelWithRecordedResult_ReturnsBadRequest()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course);
        TestData.AddResult(db, enrolment);
        var controller = TestData.AsUser(new EnrolmentsController(db), teacher);

        var action = await controller.UpdateStatus(enrolment.EnrolmentId,
            new EnrolmentStatusDto { Status = "Cancelled" });

        Assert.IsInstanceOfType(action, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task Withdraw_WithRecordedResult_ReturnsConflict()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course);
        TestData.AddResult(db, enrolment);
        var controller = TestData.AsUser(new EnrolmentsController(db), student);

        var action = await controller.Withdraw(enrolment.EnrolmentId);

        Assert.IsInstanceOfType(action, typeof(ConflictObjectResult));
        Assert.AreEqual(1, await db.Enrolments.CountAsync());
    }

    [TestMethod]
    public async Task Withdraw_OtherStudentsEnrolment_ReturnsForbid()
    {
        using var db = TestData.NewContext();
        var student = TestData.AddStudent(db, "S101");
        var other = TestData.AddStudent(db, "S102");
        var teacher = TestData.AddTeacher(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course);
        var controller = TestData.AsUser(new EnrolmentsController(db), other);

        var action = await controller.Withdraw(enrolment.EnrolmentId);

        Assert.IsInstanceOfType(action, typeof(ForbidResult));
    }

    [TestMethod]
    public async Task GetMyEnrolments_ReturnsOnlySignedInStudentsEnrolments()
    {
        using var db = TestData.NewContext();
        var student = TestData.AddStudent(db, "S101");
        var other = TestData.AddStudent(db, "S102");
        var teacher = TestData.AddTeacher(db);
        var course = TestData.AddCourse(db, teacher);
        TestData.AddEnrolment(db, student, course);
        TestData.AddEnrolment(db, other, course);
        var controller = TestData.AsUser(new EnrolmentsController(db), student);

        var action = await controller.GetMyEnrolments();

        Assert.IsInstanceOfType(action, typeof(OkObjectResult));
        Assert.AreEqual(1, ((System.Collections.IEnumerable)((OkObjectResult)action).Value!).Cast<object>().Count());
    }

    [TestMethod]
    public async Task GetEnrolment_StudentOwnsEnrolment_ReturnsOk()
    {
        using var db = TestData.NewContext();
        var student = TestData.AddStudent(db);
        var teacher = TestData.AddTeacher(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course);
        var controller = TestData.AsUser(new EnrolmentsController(db), student);

        var action = await controller.GetEnrolment(enrolment.EnrolmentId);

        Assert.IsInstanceOfType(action, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task UpdateStatus_OtherTeacher_ReturnsForbid()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db, "T101");
        var otherTeacher = TestData.AddTeacher(db, "T102");
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course, "Pending");
        var controller = TestData.AsUser(new EnrolmentsController(db), otherTeacher);

        var action = await controller.UpdateStatus(enrolment.EnrolmentId,
            new EnrolmentStatusDto { Status = "Confirmed" });

        Assert.IsInstanceOfType(action, typeof(ForbidResult));
        Assert.AreEqual("Pending", (await db.Enrolments.SingleAsync()).Status);
    }

    [TestMethod]
    public async Task Withdraw_OwnEnrolmentWithoutResult_RemovesEnrolment()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course);
        var controller = TestData.AsUser(new EnrolmentsController(db), student);

        var action = await controller.Withdraw(enrolment.EnrolmentId);

        Assert.IsInstanceOfType(action, typeof(NoContentResult));
        Assert.AreEqual(0, await db.Enrolments.CountAsync());
    }

    [TestMethod]
    public async Task GetCourseEnrolments_OwningTeacher_ReturnsCourseStudents()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher, "WEB101");
        var otherCourse = TestData.AddCourse(db, teacher, "WEB102");
        TestData.AddEnrolment(db, student, course);
        TestData.AddEnrolment(db, student, otherCourse);
        var controller = TestData.AsUser(new EnrolmentsController(db), teacher);

        var action = await controller.GetCourseEnrolments(course.CourseId);

        Assert.IsInstanceOfType(action, typeof(OkObjectResult));
        Assert.AreEqual(1, ((System.Collections.IEnumerable)((OkObjectResult)action).Value!).Cast<object>().Count());
    }
}
