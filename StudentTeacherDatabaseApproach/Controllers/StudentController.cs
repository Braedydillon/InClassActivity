using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StudentTeacherDatabaseApproach.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Demonstrates an endpoint restricted to the Student role.
    public class StudentController : Controller
    {
        [Authorize(Roles = "Student")]
        [HttpGet("student-only")]
        public IActionResult StudentOnly()
        {
            return Ok("Hello Student! You are authorized.");
        }

    }
}
