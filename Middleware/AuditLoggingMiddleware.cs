using EmployeeApp.Data;
using EmployeeApp.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace EmployeeApp.Middleware
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public AuditLoggingMiddleware(RequestDelegate next) 
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            var path = context.Request.Path.ToString();
            
            if (context.User.Identity.IsAuthenticated &&
                !path.StartsWith("/css") &&
                !path.StartsWith("/js") &&
                !path.Contains(".") &&
                !path.StartsWith("/api"))
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await userManager.FindByIdAsync(userId);

                if (user != null)
                {
                    string actionType = "VisitedPage";

                    // Identify Login
                    if (path.Equals("/Account/Login", StringComparison.OrdinalIgnoreCase))
                        actionType = "Login";

                    // Identify Logout
                    else if (path.Equals("/Account/Logout", StringComparison.OrdinalIgnoreCase))
                        actionType = "Logout";

                    // Identify Homepage redirection
                    else if (path == "/")
                        path = "/Homepage";

                    string sql = "INSERT INTO EmployeeAuditLogs (EmployeeCode, PageVisited, ActionType, ActionTime)" +
                                 "VALUES (@EmployeeCode, @PageVisited, @ActionType, @ActionTime)";

                    var parameters = new[]
                    {
                        new SqlParameter("@EmployeeCode", user.EmployeeCode),
                        new SqlParameter("@PageVisited", path),
                        new SqlParameter("@ActionType", actionType),
                        new SqlParameter("@ActionTime", DateTime.Now)
                    };

                    await dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
                }
            }
            await _next(context);
        }
    }

    public static class AuditLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuditLoggingMiddleware>();
        }
    }
}
