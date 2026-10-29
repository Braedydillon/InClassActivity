using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using practise.Models;
using practise.Services;
namespace practise.Controllers;
[Authorize(Roles = "Student")]
public class LearningController(ApiClient api) : Controller {
    public async Task<IActionResult> Index() => View(new LearningPage {
        Enrolments = await api.Get<List<Enrolment>>("api/enrolments/my"),
        Results = await api.Get<List<CourseResult>>("api/results/my")
    });
    [HttpPost] public async Task<IActionResult> Withdraw(int id) {
        try { await api.Execute(HttpMethod.Delete, $"api/enrolments/{id}"); TempData["Success"] = "Enrolment withdrawn."; }
        catch (ApiException ex) when (ex.Status == 409) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }
}
