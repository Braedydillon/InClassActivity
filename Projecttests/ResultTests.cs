using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudentTeacherDatabaseApproach.Controllers;
using StudentTeacherDatabaseApproach.DTOs;

namespace Projecttests;

[TestClass]
public class ResultTests
{
    [TestMethod]
    public async Task RecordResult_ConfirmedEnrolment_50PercentIsPass()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course);
        var controller = TestData.AsUser(new ResultsController(db), teacher);

        var action = await controller.RecordResult(enrolment.EnrolmentId,
            new CourseResultDto { FinalMark = 50m });

        Assert.AreEqual(201, ((ObjectResult)action).StatusCode);
        var result = await db.CourseResults.SingleAsync();
        Assert.AreEqual(50m, result.FinalMark);
        Assert.AreEqual("Pass", result.Outcome);
    }

    [TestMethod]
    public async Task RecordResult_49PercentIsFail()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course);
        var controller = TestData.AsUser(new ResultsController(db), teacher);

        var action = await controller.RecordResult(enrolment.EnrolmentId,
            new CourseResultDto { FinalMark = 49m });

        Assert.AreEqual(201, ((ObjectResult)action).StatusCode);
        Assert.AreEqual("Fail", (await db.CourseResults.SingleAsync()).Outcome);
    }

    [TestMethod]
    public async Task RecordResult_MarkOver100_ReturnsBadRequest()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course);
        var controller = TestData.AsUser(new ResultsController(db), teacher);

        var action = await controller.RecordResult(enrolment.EnrolmentId,
            new CourseResultDto { FinalMark = 101m });

        Assert.IsInstanceOfType(action, typeof(BadRequestObjectResult));
        Assert.AreEqual(0, await db.CourseResults.CountAsync());
    }

    [TestMethod]
    public async Task RecordResult_PendingEnrolment_ReturnsBadRequest()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course, "Pending");
        var controller = TestData.AsUser(new ResultsController(db), teacher);

        var action = await controller.RecordResult(enrolment.EnrolmentId,
            new CourseResultDto { FinalMark = 90m });

        Assert.IsInstanceOfType(action, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task RecordResult_AlreadyRecorded_ReturnsConflict()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course);
        TestData.AddResult(db, enrolment);
        var controller = TestData.AsUser(new ResultsController(db), teacher);

        var action = await controller.RecordResult(enrolment.EnrolmentId,
            new CourseResultDto { FinalMark = 70m });

        Assert.IsInstanceOfType(action, typeof(ConflictObjectResult));
        Assert.AreEqual(1, await db.CourseResults.CountAsync());
    }

    [TestMethod]
    public async Task RecordResult_NonOwningTeacher_ReturnsForbid()
    {
        using var db = TestData.NewContext();
        var ownerTeacher = TestData.AddTeacher(db, "T101");
        var differentTeacher = TestData.AddTeacher(db, "T102");
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, ownerTeacher);
        var enrolment = TestData.AddEnrolment(db, student, course);
        var controller = TestData.AsUser(new ResultsController(db), differentTeacher);

        var action = await controller.RecordResult(enrolment.EnrolmentId,
            new CourseResultDto { FinalMark = 90m });

        Assert.IsInstanceOfType(action, typeof(ForbidResult));
        Assert.AreEqual(0, await db.CourseResults.CountAsync());
    }

    [TestMethod]
    public async Task UpdateResult_NewMark_RecalculatesPassFail()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course);
        var result = TestData.AddResult(db, enrolment, 40m);
        var controller = TestData.AsUser(new ResultsController(db), teacher);

        var action = await controller.UpdateResult(result.CourseResultId,
            new CourseResultDto { FinalMark = 75m });

        Assert.IsInstanceOfType(action, typeof(OkObjectResult));
        var updated = await db.CourseResults.SingleAsync();
        Assert.AreEqual(75m, updated.FinalMark);
        Assert.AreEqual("Pass", updated.Outcome);
    }

    [TestMethod]
    public async Task GetMyResults_OnlyReturnsSignedInStudentsResults()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db, "S101");
        var otherStudent = TestData.AddStudent(db, "S102");
        var course = TestData.AddCourse(db, teacher);
        TestData.AddResult(db, TestData.AddEnrolment(db, student, course), 65m);
        TestData.AddResult(db, TestData.AddEnrolment(db, otherStudent, course), 85m);
        var controller = TestData.AsUser(new ResultsController(db), student);

        var action = await controller.GetMyResults();

        Assert.IsInstanceOfType(action, typeof(OkObjectResult));
        // The controller returns an anonymous list; verify its length without relying on its type.
        var value = ((OkObjectResult)action).Value;
        Assert.IsNotNull(value);
        Assert.AreEqual(1, ((System.Collections.IEnumerable)value).Cast<object>().Count());
    }

    [TestMethod]
    public async Task RecordResult_NegativeMark_ReturnsBadRequest()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course);
        var controller = TestData.AsUser(new ResultsController(db), teacher);

        var action = await controller.RecordResult(enrolment.EnrolmentId,
            new CourseResultDto { FinalMark = -1m });

        Assert.IsInstanceOfType(action, typeof(BadRequestObjectResult));
        Assert.AreEqual(0, await db.CourseResults.CountAsync());
    }

    [TestMethod]
    public async Task RecordResult_UnknownEnrolment_ReturnsNotFound()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var controller = TestData.AsUser(new ResultsController(db), teacher);

        var action = await controller.RecordResult(999, new CourseResultDto { FinalMark = 50m });

        Assert.IsInstanceOfType(action, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task UpdateResult_NonOwningTeacher_ReturnsForbid()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db, "T101");
        var other = TestData.AddTeacher(db, "T102");
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course);
        var result = TestData.AddResult(db, enrolment, 66m);
        var controller = TestData.AsUser(new ResultsController(db), other);

        var action = await controller.UpdateResult(result.CourseResultId,
            new CourseResultDto { FinalMark = 100m });

        Assert.IsInstanceOfType(action, typeof(ForbidResult));
        Assert.AreEqual(66m, (await db.CourseResults.SingleAsync()).FinalMark);
    }

    [TestMethod]
    public async Task UpdateResult_MarkAbove100_ReturnsBadRequest()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher);
        var enrolment = TestData.AddEnrolment(db, student, course);
        var result = TestData.AddResult(db, enrolment, 66m);
        var controller = TestData.AsUser(new ResultsController(db), teacher);

        var action = await controller.UpdateResult(result.CourseResultId,
            new CourseResultDto { FinalMark = 101m });

        Assert.IsInstanceOfType(action, typeof(BadRequestObjectResult));
        Assert.AreEqual(66m, (await db.CourseResults.SingleAsync()).FinalMark);
    }

    [TestMethod]
    public async Task GetCourseResults_OwningTeacher_OnlyShowsCourseResults()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var student = TestData.AddStudent(db);
        var course = TestData.AddCourse(db, teacher, "WEB101");
        var otherCourse = TestData.AddCourse(db, teacher, "WEB102");
        TestData.AddResult(db, TestData.AddEnrolment(db, student, course));
        TestData.AddResult(db, TestData.AddEnrolment(db, student, otherCourse));
        var controller = TestData.AsUser(new ResultsController(db), teacher);

        var action = await controller.GetCourseResults(course.CourseId);

        Assert.IsInstanceOfType(action, typeof(OkObjectResult));
        Assert.AreEqual(1, ((System.Collections.IEnumerable)((OkObjectResult)action).Value!).Cast<object>().Count());
    }
}
