using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace practise.Services;

public sealed class ApiException(int status, string message) : Exception(message)
{
    public int Status { get; } = status;
}

// API cookies belong to the current server-side session, never a shared HttpClient cookie jar.
public sealed class ApiClient(HttpClient client, IHttpContextAccessor accessor)
{
    private ISession Session => accessor.HttpContext!.Session;
    public void ClearSession() => Session.Remove("ApiCookies");
    public async Task<T> Get<T>(string path) {
        using var response = await Send(HttpMethod.Get, path);
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }
    public async Task Execute(HttpMethod method, string path, object? body = null) {
        using var response = await Send(method, path, body);
    }
    private async Task<HttpResponseMessage> Send(HttpMethod method, string path, object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);
        var cookies = JsonSerializer.Deserialize<Dictionary<string, string>>(Session.GetString("ApiCookies") ?? "{}")!;
        if (cookies.Count > 0) request.Headers.TryAddWithoutValidation("Cookie", string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}")));
        if (body is not null) request.Content = JsonContent.Create(body);
        HttpResponseMessage response;
        try { response = await client.SendAsync(request, accessor.HttpContext!.RequestAborted); }
        catch (HttpRequestException) { throw new ApiException(503, "The API is unavailable. Start the StudentTeacherDatabaseApproach project and try again."); }
        catch (TaskCanceledException) { throw new ApiException(503, "The API took too long to respond. Check that the API and SQL Server are running, then try again."); }
        if (response.Headers.TryGetValues("Set-Cookie", out var values)) {
            foreach (var value in values) {
                var pair = value.Split(';', 2)[0].Split('=', 2);
                if (pair.Length == 2 && pair[0].StartsWith(".AspNetCore.Cookies", StringComparison.Ordinal)) cookies[pair[0]] = pair[1];
            }
            Session.SetString("ApiCookies", JsonSerializer.Serialize(cookies));
        }
        if (response.IsSuccessStatusCode) return response;
        var status = (int)response.StatusCode;
        var message = status switch {
            401 => "Your session has expired or your email/password is incorrect. Please sign in again.",
            403 => "You do not have permission to perform this action.",
            404 => "The requested item could not be found.",
            >= 500 => "The API could not complete the request. Check its database connection and try again.",
            >= 300 and < 400 => "The API redirected the request. Set Api:BaseUrl to the API's active HTTPS address, or run its HTTP launch profile.",
            _ => "The request could not be completed."
        };
        if (status is 400 or 409) {
            var text = await response.Content.ReadAsStringAsync();
            try {
                using var json = JsonDocument.Parse(text);
                var root = json.RootElement;
                if (root.ValueKind == JsonValueKind.String) message = root.GetString()!;
                else if (root.TryGetProperty("errors", out var errors)) message = string.Join(" ", errors.EnumerateObject().SelectMany(p => p.Value.EnumerateArray().Select(v => v.GetString())));
                else if (root.TryGetProperty("title", out var title)) message = title.GetString()!;
            } catch (JsonException) { if (!text.Contains('<') && text.Length < 1000) message = text; }
        }
        response.Dispose();
        throw new ApiException(status, message);
    }
}

public sealed class ApiExceptionFilter(Microsoft.AspNetCore.Mvc.ModelBinding.IModelMetadataProvider metadata) : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context) {
        if (context.Exception is not ApiException error) return;
        if (error.Status == 401) {
            context.HttpContext.Session.Clear();
            await context.HttpContext.SignOutAsync();
            context.Result = new RedirectToActionResult("Login", "Account", new { expired = true });
        } else {
            var viewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(metadata, context.ModelState);
            viewData["ApiError"] = error.Message;
            context.Result = new ViewResult { ViewName = "ApiError", ViewData = viewData, StatusCode = error.Status };
        }
        context.ExceptionHandled = true;
    }
}

