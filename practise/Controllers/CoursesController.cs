using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using practise.Models;
using practise.Services;
namespace practise.Controllers;

[Authorize]
public class CoursesController(ApiClient api) : Controller
{
    public async Task<IActionResult> Index(string? search, bool mine = false) {
        mine = mine && User.IsInRole("Teacher");
        var courses = await api.Get<List<Course>>(mine ? "api/courses/my" : "api/courses");
        ViewData["Search"] = search; ViewData["Mine"] = mine;
        if (!string.IsNullOrWhiteSpace(search)) courses = courses.Where(c => $"{c.Code} {c.Title} {c.CategoryName}".Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        return View(courses);
    }
    public async Task<IActionResult> Details(int id) {
        var course = await api.Get<Course>($"api/courses/{id}");
        var page = new CourseDetails { Course = course, CanManage = User.IsInRole("Teacher") && User.FindFirst("TeacherProfileId")?.Value == course.TeacherProfileId.ToString() };
        if (page.CanManage) {
            page.Enrolments = await api.Get<List<Enrolment>>($"api/courses/{id}/enrolments");
            page.Results = await api.Get<List<CourseResult>>($"api/courses/{id}/results");
        }
        if (User.IsInRole("Student")) page.IsEnrolled = (await api.Get<List<Enrolment>>("api/enrolments/my")).Any(e => e.CourseId == id);
        return View(page);
    }
    [Authorize(Roles = "Teacher"), HttpGet] public async Task<IActionResult> Edit(int id = 0) {
        CourseForm form = id == 0 ? new CourseForm() : await api.Get<Course>($"api/courses/{id}");
        if (form is Course course && User.FindFirst("TeacherProfileId")?.Value != course.TeacherProfileId.ToString()) return Forbid();
        form.Categories = await api.Get<List<Category>>("api/categories");
        return View(form);
    }
    [Authorize(Roles = "Teacher"), HttpPost] public async Task<IActionResult> Edit(CourseForm form) {
        if (ModelState.IsValid) {
            try {
                await api.Execute(form.CourseId == 0 ? HttpMethod.Post : HttpMethod.Put, form.CourseId == 0 ? "api/courses" : $"api/courses/{form.CourseId}", new { form.Code, form.Title, form.Description, form.CourseCategoryId });
                TempData["Success"] = "Course saved."; return RedirectToAction(nameof(Index), new { mine = true });
            } catch (ApiException ex) when (ex.Status is 400 or 409) { ModelState.AddModelError("", ex.Message); }
        }
        form.Categories = await api.Get<List<Category>>("api/categories");
        return View(form);
    }
    [Authorize(Roles = "Teacher"), HttpPost] public async Task<IActionResult> Delete(int id) {
        try { await api.Execute(HttpMethod.Delete, $"api/courses/{id}"); TempData["Success"] = "Course deleted."; }
        catch (ApiException ex) when (ex.Status == 409) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index), new { mine = true });
    }
    [Authorize(Roles = "Student"), HttpPost] public async Task<IActionResult> Enrol(int id) {
        try { await api.Execute(HttpMethod.Post, $"api/courses/{id}/enrol"); TempData["Success"] = "Enrolment requested. Your teacher can now confirm it."; }
        catch (ApiException ex) when (ex.Status is 400 or 409) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { id });
    }
    [Authorize(Roles = "Teacher"), HttpPost] public async Task<IActionResult> Status(int id, int courseId, string status) {
        if (status is not ("Pending" or "Confirmed" or "Cancelled")) TempData["Error"] = "Choose a valid enrolment status.";
        else {
            try { await api.Execute(HttpMethod.Put, $"api/enrolments/{id}/status", new { status }); TempData["Success"] = "Enrolment updated."; }
            catch (ApiException ex) when (ex.Status is 400 or 409) { TempData["Error"] = ex.Message; }
        }
        return RedirectToAction(nameof(Details), new { id = courseId });
    }
    [Authorize(Roles = "Teacher"), HttpPost] public async Task<IActionResult> Mark(int enrolmentId, int resultId, int courseId, decimal? finalMark) {
        if (!ModelState.IsValid || finalMark is null or < 0 or > 100) TempData["Error"] = "Enter a mark between 0 and 100.";
        else {
            try {
                await api.Execute(resultId == 0 ? HttpMethod.Post : HttpMethod.Put, resultId == 0 ? $"api/enrolments/{enrolmentId}/result" : $"api/results/{resultId}", new { finalMark });
                TempData["Success"] = "Result saved.";
            } catch (ApiException ex) when (ex.Status is 400 or 409) { TempData["Error"] = ex.Message; }
        }
        return RedirectToAction(nameof(Details), new { id = courseId });
    }
}

