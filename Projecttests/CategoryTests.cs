using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudentTeacherDatabaseApproach.Controllers;
using StudentTeacherDatabaseApproach.DTOs;

namespace Projecttests;

[TestClass]
public class CategoryTests
{
    [TestMethod]
    public async Task CreateCategory_NewName_Returns201AndPersists()
    {
        using var db = TestData.NewContext();
        var action = await new CategoriesController(db).CreateCategory(new CourseCategoryDto
        {
            Name = "Science", Description = "Science courses"
        });

        Assert.AreEqual(201, ((ObjectResult)action).StatusCode);
        Assert.AreEqual("Science", (await db.CourseCategories.SingleAsync()).Name);
    }

    [TestMethod]
    public async Task CreateCategory_DuplicateName_ReturnsConflict()
    {
        using var db = TestData.NewContext();
        TestData.AddCategory(db, "Science");

        var action = await new CategoriesController(db).CreateCategory(new CourseCategoryDto
        {
            Name = "Science"
        });

        Assert.IsInstanceOfType(action, typeof(ConflictObjectResult));
        Assert.AreEqual(1, await db.CourseCategories.CountAsync());
    }

    [TestMethod]
    public async Task UpdateCategory_UnknownId_ReturnsNotFound()
    {
        using var db = TestData.NewContext();
        var action = await new CategoriesController(db).UpdateCategory(999, new CourseCategoryDto
        {
            Name = "Art"
        });
        Assert.IsInstanceOfType(action, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task DeleteCategory_WithLinkedCourse_ReturnsConflict()
    {
        using var db = TestData.NewContext();
        var teacher = TestData.AddTeacher(db);
        var course = TestData.AddCourse(db, teacher);

        var action = await new CategoriesController(db).DeleteCategory(course.CourseCategoryId);

        Assert.IsInstanceOfType(action, typeof(ConflictObjectResult));
        Assert.IsTrue(await db.CourseCategories.AnyAsync(c => c.CourseCategoryId == course.CourseCategoryId));
    }

    [TestMethod]
    public async Task DeleteCategory_WithoutCourses_ReturnsNoContent()
    {
        using var db = TestData.NewContext();
        var category = TestData.AddCategory(db);

        var action = await new CategoriesController(db).DeleteCategory(category.CourseCategoryId);

        Assert.IsInstanceOfType(action, typeof(NoContentResult));
        Assert.AreEqual(0, await db.CourseCategories.CountAsync());
    }

    [TestMethod]
    public async Task UpdateCategory_ExistingCategory_ChangesNameAndDescription()
    {
        using var db = TestData.NewContext();
        var category = TestData.AddCategory(db);
        var action = await new CategoriesController(db).UpdateCategory(category.CourseCategoryId,
            new CourseCategoryDto { Name = "Updated", Description = "New description" });

        Assert.IsInstanceOfType(action, typeof(OkObjectResult));
        var saved = await db.CourseCategories.SingleAsync();
        Assert.AreEqual("Updated", saved.Name);
        Assert.AreEqual("New description", saved.Description);
    }

    [TestMethod]
    public async Task UpdateCategory_ToExistingName_ReturnsConflictAndPreservesOriginal()
    {
        using var db = TestData.NewContext();
        var first = TestData.AddCategory(db, "Science");
        var second = TestData.AddCategory(db, "Maths");
        var action = await new CategoriesController(db).UpdateCategory(second.CourseCategoryId,
            new CourseCategoryDto { Name = first.Name });

        Assert.IsInstanceOfType(action, typeof(ConflictObjectResult));
        Assert.AreEqual("Maths", (await db.CourseCategories.FindAsync(second.CourseCategoryId))!.Name);
    }

    [TestMethod]
    public async Task GetCategories_ReturnsAllCategories()
    {
        using var db = TestData.NewContext();
        TestData.AddCategory(db, "Programming");
        TestData.AddCategory(db, "Mathematics");
        var action = await new CategoriesController(db).GetCategories();

        Assert.IsInstanceOfType(action, typeof(OkObjectResult));
        var rows = (System.Collections.IEnumerable)((OkObjectResult)action).Value!;
        Assert.AreEqual(2, rows.Cast<object>().Count());
    }
}
