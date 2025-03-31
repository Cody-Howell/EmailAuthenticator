namespace EmailAuthenticator;

using Microsoft.AspNetCore.Http;
using System.Net.Http;

public class IdentityMiddleware(RequestDelegate next, AuthService service, IIDMiddlewareConfig config) {
    public async Task InvokeAsync(HttpContext context) {
        if (config.Paths.Contains(context.Request.Path)) {
            await next(context);
        } else {
            // Validate user here
            string? email = context.Request.Headers["Email-Auth_Email"];
            string? key = context.Request.Headers["Email-Auth_ApiKey"];
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(key)) {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Missing header(s). Requires an \"Email-Auth_Email\" and \"Email-Auth_ApiKey\" header.");
                return;
            }

            DateTime? output = new DateTime();
            try {
                output = service.IsValidApiKey(email, key);
            } catch {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid API key");
                return;
            } 


            TimeSpan? timeBetween = DateTime.Now.ToUniversalTime() - output;
            if (timeBetween < config.ExpirationDate) {
                await next(context);
            } else {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Time has run out. Please sign in again.");
            }
        }
    }
}

