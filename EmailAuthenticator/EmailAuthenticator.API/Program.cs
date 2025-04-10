using EmailAuthenticator;
using Npgsql;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

var connString = builder.Configuration["DOTNET_DATABASE_STRING"] ?? throw new InvalidOperationException("Connection string for database not found.");
Console.WriteLine("Connection String: " + connString);
builder.Services.AddSingleton<IDbConnection>(provider => {
    return new NpgsqlConnection(connString);
});
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<IIDMiddlewareConfig, IDMiddlewareConfig>();
builder.Services.AddSingleton<IEmailService, EmailService>();

var app = builder.Build();

app.UseMiddleware<IdentityMiddleware>();
app.UseRouting();

app.MapGet("/api/users", (AuthService service) => service.GetAllUsers());

app.MapPost("/api/signin", (AuthService service, string email) => {
    return service.AddUser(email);
});

app.MapGet("/api/signin/validate", (AuthService service, string email, string token) => {
    service.Validate(email, token);
    return Results.Redirect("/signedin");
});

app.MapGet("/api/user/{email}", (AuthService service, string email) => {
    return service.GetUser(email);
});

app.MapGet("/signedin", () => "Thanks for signing in!");

app.MapGet("/health", () => "Health check");

app.MapGet("/api/signin/delete/{email}", (AuthService service, string email) => {
    service.DeleteUser(email);
});

app.MapGet("/api/signout/global/{email}", (AuthService service, string email) => {
    service.GlobalSignOut(email);
});

app.MapGet("/api/signout/{email}", (HttpRequest request, AuthService service, string email) => {
    string key = request.Headers["Email-Auth_ApiKey"]!; // Is not null, validated in middleware
    service.KeySignOut(email, key);
});

app.Run();


internal class IDMiddlewareConfig : IIDMiddlewareConfig {
    public List<string> Paths => new List<string>() { "/health", "/api/signin", "/api/signin/validate", "/signedin" };

    public TimeSpan? ExpirationDate => new TimeSpan(90, 0, 0, 0); // 3 Months
    //public TimeSpan? ExpirationDate => null; // infinite


    public TimeSpan? ReValidationDate => new TimeSpan(10, 0, 0, 0); // 10 days 

}

internal class EmailService : IEmailService {
    public async Task SendValidationEmail(string email, string validationToken) {
        Console.WriteLine($"http://localhost:5092/api/signin/validate?email={email}&token={validationToken}");
    }
}