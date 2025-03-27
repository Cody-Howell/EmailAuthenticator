namespace EmailAuthenticator;

using Microsoft.AspNetCore.Http;
using System.Net.Http;

public class IdentityMiddleware(RequestDelegate next, AuthService service, IValidPaths path) {
    private readonly List<string> paths = path.Paths;
    public async Task InvokeAsync(HttpContext context) {
        if (paths.Contains(context.Request.Path)) {
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
            if (users.Any(u => u.Email == user)) {
                await next(context);
            } else {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
            }
        }
    }
}

