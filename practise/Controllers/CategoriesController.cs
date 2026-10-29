using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using practise.Models;
using practise.Services;
namespace practise.Controllers;
[Authorize]
public class CategoriesController(ApiClient api) : Controller {
    public async Task<IActionResult> Index() => View(await api.Get<List<Category>>("api/categories"));
    [Authorize(Roles = "Teacher"), HttpGet] public async Task<IActionResult> Edit(int id = 0) {
        if (id == 0) return View(new Category());
        var category = (await api.Get<List<Category>>("api/categories")).FirstOrDefault(c => c.CourseCategoryId == id);
        return category is null ? NotFound() : View(category);
    }
    [Authorize(Roles = "Teacher"), HttpPost] public async Task<IActionResult> Edit(Category form) {
        if (ModelState.IsValid) {
            try {
                await api.Execute(form.CourseCategoryId == 0 ? HttpMethod.Post : HttpMethod.Put, form.CourseCategoryId == 0 ? "api/categories" : $"api/categories/{form.CourseCategoryId}", form);
                TempData["Success"] = "Category saved."; return RedirectToAction(nameof(Index));
            } catch (ApiException ex) when (ex.Status is 400 or 409) { ModelState.AddModelError("", ex.Message); }
        }
        return View(form);
    }
    [Authorize(Roles = "Teacher"), HttpPost] public async Task<IActionResult> Delete(int id) {
        try { await api.Execute(HttpMethod.Delete, $"api/categories/{id}"); TempData["Success"] = "Category deleted."; }
        catch (ApiException ex) when (ex.Status == 409) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }
}
