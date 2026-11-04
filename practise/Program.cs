using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using practise.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options => {
    // Require antiforgery validation for modifying MVC requests across all controllers.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add<ApiExceptionFilter>();
});
// Session data is stored in this process and is lost when the MVC application restarts.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.Cookie.Name = "StudentTeacher.Mvc.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
    options.Cookie.Name = "StudentTeacher.Mvc.Auth";
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Denied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<ApiClient>(client => {
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5243/");
    client.Timeout = TimeSpan.FromSeconds(15);
// Disable shared cookies to isolate users; surface redirects as API responses for error handling.
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false });
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
// Use dot-decimal form values consistently, including course marks.
app.UseRequestLocalization(new RequestLocalizationOptions().SetDefaultCulture("en-US").AddSupportedCultures("en-US").AddSupportedUICultures("en-US"));
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
