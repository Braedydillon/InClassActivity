using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudentTeacherDatabaseApproach.Controllers;

namespace Projecttests;

/// <summary>
/// Checks security attributes are declared on endpoints; actual middleware
/// authentication/authorization is outside the scope of controller unit tests.
/// </summary>
[TestClass]
public class AuthorizationTests
{
    [DataTestMethod]
    [DataRow(typeof(CategoriesController), "CreateCategory", "Teacher")]
    [DataRow(typeof(CategoriesController), "UpdateCategory", "Teacher")]
    [DataRow(typeof(CategoriesController), "DeleteCategory", "Teacher")]
    [DataRow(typeof(CoursesController), "CreateCourse", "Teacher")]
    [DataRow(typeof(CoursesController), "UpdateCourse", "Teacher")]
    [DataRow(typeof(CoursesController), "DeleteCourse", "Teacher")]
    [DataRow(typeof(EnrolmentsController), "Enrol", "Student")]
    [DataRow(typeof(EnrolmentsController), "Withdraw", "Student")]
    [DataRow(typeof(EnrolmentsController), "UpdateStatus", "Teacher")]
    [DataRow(typeof(ResultsController), "RecordResult", "Teacher")]
    [DataRow(typeof(ResultsController), "GetMyResults", "Student")]
    [DataRow(typeof(ResultsController), "UpdateResult", "Teacher")]
    public void ProtectedEndpoints_RequireExpectedRole(Type controllerType,
        string actionName, string expectedRole)
    {
        var method = controllerType.GetMethod(actionName);
        Assert.IsNotNull(method, $"Missing endpoint: {controllerType.Name}.{actionName}");
        var roleRequirement = method.GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>().SingleOrDefault();
        Assert.IsNotNull(roleRequirement, $"Missing [Authorize] on {controllerType.Name}.{actionName}");
        Assert.AreEqual(expectedRole, roleRequirement.Roles);
    }

    [DataTestMethod]
    [DataRow(typeof(CoursesController))]
    [DataRow(typeof(EnrolmentsController))]
    [DataRow(typeof(ResultsController))]
    public void Controllers_RequireAuthentication(Type controllerType)
    {
        Assert.IsTrue(controllerType.IsDefined(typeof(AuthorizeAttribute), true),
            $"Missing [Authorize] on {controllerType.Name}");
    }
}
