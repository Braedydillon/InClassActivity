using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudentTeacherDatabaseApproach.Controllers;

namespace Projecttests
{
    /// <summary>
    /// Checks that protected controller endpoints declare the expected roles.
    /// Authentication and authorization middleware are tested separately.
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
        public void ProtectedEndpoints_RequireExpectedRole(
            Type controllerType,
            string actionName,
            string expectedRole)
        {
            MethodInfo[] methods = controllerType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(method => method.Name == actionName)
                .ToArray();

            Assert.IsNotNull(
                methods,
                string.Format(
                    "Missing endpoint: {0}.{1}",
                    controllerType.Name,
                    actionName));

            Assert.IsTrue(
                methods.Length > 0,
                string.Format(
                    "Missing endpoint: {0}.{1}",
                    controllerType.Name,
                    actionName));

            AuthorizeAttribute? roleRequirement = methods
                .SelectMany(method => method
                    .GetCustomAttributes(typeof(AuthorizeAttribute), true)
                    .Cast<AuthorizeAttribute>())
                .SingleOrDefault();

            Assert.IsNotNull(
                roleRequirement,
                string.Format(
                    "Missing [Authorize] on {0}.{1}",
                    controllerType.Name,
                    actionName));

            Assert.AreEqual(expectedRole, roleRequirement.Roles);
        }

        [DataTestMethod]
        [DataRow(typeof(CategoriesController))]
        [DataRow(typeof(CoursesController))]
        [DataRow(typeof(EnrolmentsController))]
        [DataRow(typeof(ResultsController))]
        public void Controllers_RequireAuthentication(Type controllerType)
        {
            Assert.IsTrue(
                controllerType.IsDefined(typeof(AuthorizeAttribute), true),
                string.Format(
                    "Missing [Authorize] on {0}",
                    controllerType.Name));
        }
    }
}
