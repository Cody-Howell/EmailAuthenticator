namespace EmailAuthenticator;

using Microsoft.AspNetCore.Http;
using System.Net.Http;

public class IdentityMiddleware(RequestDelegate next, AuthService service) {
    public async Task InvokeAsync(HttpContext context) {
        if (context.Request.Path == "/api/health" ||
            context.Request.Path == "/api/signin") {
            await next(context);
        } else {
            // Validate user here
            string? user = context.Request.Headers["User"];
            string? key = context.Request.Headers["ApiKey"];
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(key)) {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Missing header(s)");
                return;
            }

            List<User> users = service.GetAllUsers().ToList();
            if (users.Any(u => u.Username == user && u.ApiKey == key)) {
                await next(context);
            } else {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
            }
        }
    }
}

