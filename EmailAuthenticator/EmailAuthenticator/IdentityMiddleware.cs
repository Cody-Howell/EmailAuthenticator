﻿namespace EmailAuthenticator;

using Microsoft.AspNetCore.Http;
using System.Net.Http;

/// <summary>
/// Identity Middleware relies on the <c>IIDMiddlewareConfig</c> to be injected through DI, as well as the AuthService. 
/// For any error, it will throw a <c>401</c> HTTP code with a string (of which 3 are user-friendly). Make sure 
/// the headers always contain a little bit of information, as the 4th is developer-intended. 
/// <br/> <br/>
/// This takes every path not in Paths of the config and checks for email and API Key headers. If they are 
/// null or empty, the response will give you the exact syntax. <br/>
/// Afterwards, they will validate that you have a valid API key, and return a short, helpful message if not so. <br/>
/// Finally, if the ExpirationDate is not null, it will calculate the time between. If it's over the expiration date, 
/// it will remove that key. If it's under but over the re-auth time (also assuming config), it will 
/// reset the expiration date. Then it will let the response pass. 
/// </summary>
public class IdentityMiddleware(RequestDelegate next, AuthService service, IIDMiddlewareConfig config) {
    /// <summary>
    /// 
    /// </summary>
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
                if (output is null) {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("API key is not validated.");
                    return;
                }
            } catch {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API key does not exist.");
                return;
            } 


            TimeSpan? timeBetween = DateTime.Now.ToUniversalTime() - output;
            if (timeBetween < config.ExpirationDate) {
                if (config.ReValidationDate is not null &&
                    timeBetween > config.ReValidationDate) {
                    service.ReValidate(email, key);
                }

                await next(context);
            } else {
                service.KeySignOut(email, key);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Time has run out. Please sign in again.");
            }
        }
    }
}

