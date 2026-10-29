# StudySpace MVC frontend

The `practise` project is a .NET 10 MVC frontend for the existing `StudentTeacherDatabaseApproach` API. All application data is read and written through HTTP API calls; MVC does not connect directly to SQL Server.

## Start both projects

From the solution folder, open two terminals:

```powershell
dotnet run --project .\StudentTeacherDatabaseApproach\StudentTeacherDatabaseApproach.csproj --launch-profile https
```

```powershell
dotnet run --project .\practise\practise.csproj --launch-profile http
```

- MVC application: http://localhost:5206
- API Swagger: https://localhost:7244/swagger

In Visual Studio, configure **Multiple startup projects** and set both `StudentTeacherDatabaseApproach` and `practise` to **Start**. Choose the **https** launch profile for the API and **http** for MVC. Run only one instance of each project. The API requires SQL Server and the existing `StudentTeacherDB` database.

`practise/appsettings.json` contains `Api:BaseUrl`. The default is `https://localhost:7244/` and requires the API HTTPS profile and a trusted development certificate. If you deliberately run the API HTTP-only profile, set it to `http://localhost:5243/`. Certificate validation is not disabled.

## Walk through the application

1. Register a Teacher, including a unique employee number, then sign in.
2. Create a category if needed, then create a course from **My teaching**.
3. In another browser profile or private window, register a Student with a unique student number.
4. As the student, explore courses and request enrolment. **My learning** shows the pending request.
5. As the teacher, open the course and confirm the enrolment, then save a mark between 0 and 100.
6. Refresh **My learning** as the student to see the result.

Students can withdraw before a result is recorded. Teachers can edit their own courses, correct results, and delete courses that have no enrolments. Category management is available to teachers. Both roles can edit their profile names and sign out.

## Structure

- `Controllers/AccountController.cs`: registration, login, logout and profile forms.
- `Controllers/CoursesController.cs`: catalogue, course details, course management, enrolment status and marks.
- `Controllers/LearningController.cs`: student enrolments, withdrawal and results.
- `Controllers/CategoriesController.cs`: category management.
- `Models/PortalModels.cs`: typed API responses and validated form models.
- `Services/ApiClient.cs`: HTTP API client and readable error handling.
- `Views/`: strongly typed Razor pages.
- `wwwroot/css/site.css`: responsive layout using local assets.

The API authentication cookie is stored in the current user's server-side MVC session. The shared HTTP handler has automatic cookie handling disabled, preventing one user's login from being reused by another. The MVC authentication cookie has its own name. Every modifying MVC form requires an antiforgery token, and role restrictions are enforced in both MVC and the API. API ownership checks remain authoritative.

Sessions expire after 30 minutes of inactivity and are lost when the MVC process restarts. API errors produce readable messages. The API is the source of truth for data and permissions. This example uses the API's existing self-service Teacher registration; production systems should define a teacher approval policy before public deployment.

## Checks

With both applications running on their normal HTTP ports, use PowerShell 7:

```powershell
.\scripts\Test-Mvc.ps1
```

This runs 25 HTTP checks covering both roles, form protection, course creation, enrolment, decimal marks, corrections, deletion conflicts, profile changes, session isolation and logout. It creates uniquely labelled `mvctest-*` demo accounts and records, and writes their identifiers to `scripts/mvc-test-records.json`; it does not delete existing application data. To test alternate ports, pass `-BaseUrl` and `-ApiUrl`.

Validation performed during implementation: successful build with zero warnings/errors, all 25 checks passed, and browser review of the landing and registration pages.
