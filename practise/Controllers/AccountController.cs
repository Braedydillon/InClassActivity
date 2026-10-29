using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using practise.Models;
using practise.Services;
namespace practise.Controllers;

public class AccountController(ApiClient api) : Controller
{
    [HttpGet] public IActionResult Login(bool expired = false) {
        if (expired) ViewData["Notice"] = "Your session ended. Please sign in again.";
        return View(new LoginForm());
    }
    [HttpPost] public async Task<IActionResult> Login(LoginForm form) {
        if (ModelState.IsValid) {
            api.ClearSession();
            try {
                await api.Execute(HttpMethod.Post, "api/auth/login", form);
                var profile = await api.Get<AccountProfile>("api/auth/profile");
                var identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, profile.UserId.ToString()),
                    new Claim(ClaimTypes.Name, profile.Email),
                    new Claim(ClaimTypes.Role, profile.Role),
                    new Claim("TeacherProfileId", profile.Profile.TeacherProfileId.ToString())
                }, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
                return RedirectToAction("Index", "Courses");
            } catch (ApiException ex) { api.ClearSession(); ModelState.AddModelError("", ex.Message); }
        }
        form.Password = "";
        ModelState.Remove(nameof(form.Password));
        return View(form);
    }
    [HttpGet] public IActionResult Register() => View(new RegisterForm());
    [HttpPost] public async Task<IActionResult> Register(RegisterForm form) {
        if (ModelState.IsValid) {
            try {
                await api.Execute(HttpMethod.Post, "api/auth/register", form);
                TempData["Success"] = "Your account is ready. Sign in to get started.";
                return RedirectToAction(nameof(Login));
            } catch (ApiException ex) { ModelState.AddModelError("", ex.Message); }
        }
        form.Password = form.ConfirmPassword = "";
        ModelState.Remove(nameof(form.Password)); ModelState.Remove(nameof(form.ConfirmPassword));
        return View(form);
    }
    [Authorize, HttpPost] public async Task<IActionResult> Logout() {
        try { await api.Execute(HttpMethod.Post, "api/auth/logout"); } catch (ApiException) { /* Local logout must still succeed when the API is offline. */ }
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
    [Authorize, HttpGet] public async Task<IActionResult> Profile() {
        var profile = await api.Get<AccountProfile>("api/auth/profile");
        ViewData["Number"] = profile.Profile.StudentNumber ?? profile.Profile.EmployeeNumber;
        return View(new ProfileForm { FirstName = profile.Profile.FirstName, LastName = profile.Profile.LastName });
    }
    [Authorize, HttpPost] public async Task<IActionResult> Profile(ProfileForm form) {
        if (ModelState.IsValid) {
            try { await api.Execute(HttpMethod.Put, "api/auth/profile", form); TempData["Success"] = "Profile updated."; return RedirectToAction(nameof(Profile)); }
            catch (ApiException ex) when (ex.Status is 400 or 409) { ModelState.AddModelError("", ex.Message); }
        }
        return View(form);
    }
    public IActionResult Denied() { Response.StatusCode = 403; return View(); }
}
