using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StudentTeacherDatabaseApproach.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Demonstrates an endpoint that requires login without requiring a particular role.
    public class DashBoardController : ControllerBase
    {
        [Authorize] // any logged-in user, regardless of role
        [HttpGet("logged-in-only")]
        public IActionResult LoggedInOnly()
        {
            return Ok("You are logged in, but this doesn't check your role.");
        }
    }
}