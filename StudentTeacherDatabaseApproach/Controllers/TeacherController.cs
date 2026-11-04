using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StudentTeacherDatabaseApproach.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Demonstrates an endpoint restricted to the Teacher role.
    public class TeacherController : Controller
    {
        [Authorize(Roles = "Teacher")]
        [HttpGet("teacher-only")]
        public IActionResult TeacherOnly()
        {
            return Ok("Hello Teacher! You are authorized.");
        }
    }
}
