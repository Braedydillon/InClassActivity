using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StudentTeacherDatabaseApproach.Data;

namespace StudentTeacherDatabaseApproach
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Register the Database-First DbContext
            builder.Services.AddDbContext<StudentTeacherDbContext>(
                options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString(
                            "DefaultConnection")));

            // Configure cookie authentication
            builder.Services
                .AddAuthentication(
                    CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    // Return HTTP error codes so API clients do not receive an HTML login redirect.
                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };

                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    };
                });


            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            //enable authorization
            builder.Services.AddAuthorization();

            //configure Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            //Authentication must run before authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
