using EmailAuthenticator;
using Npgsql;
using System.Data;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connString = builder.Configuration["DOTNET_DATABASE_STRING"] ?? throw new InvalidOperationException("Connection string for database not found.");
Console.WriteLine("Connection String: " + connString);
builder.Services.AddSingleton<IDbConnection>(provider =>
{
    return new NpgsqlConnection(connString);
});
builder.Services.AddSingleton<AuthService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseMiddleware<IdentityMiddleware>();
app.UseRouting();

app.MapGet("/api/users", (AuthService service) => service.GetAllUsers());
app.MapPost("/api/signin", (AuthService service, User user) =>
{
    List<User> users = service.GetAllUsers().ToList();
    if (users.Any(u => u.Username == user.Username)) {
        string key = StringHelper.GenerateRandomString(20);
        service.UpdateApiKey(user.Username, key);
        return key;
    } else {
        return "";
    }
});

app.MapPost("/api/signin/add", (AuthService service, string email) =>
{
    service.AddUser(new User { Email = email });
    return "Success";
});

app.MapGet("/api/signin/delete/{user}", (AuthService service, string email) =>
{
    service.DeleteUser(email);
});

app.Run();


