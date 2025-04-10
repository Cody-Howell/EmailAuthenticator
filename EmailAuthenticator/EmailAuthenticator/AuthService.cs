
using Dapper;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace EmailAuthenticator;

/// <summary>
/// Service implementation to handle the database. Runs through Dapper.
/// </summary>
public class AuthService(IDbConnection conn, IEmailService service) {
    /// <summary>
    /// Adds a new user if one doesn't already exists and returns the API key the user should use. 
    /// </summary>
    /// <returns>New API key or "Invalid Email."</returns>
    public async Task<string> AddUser(string email) {
        if (StringHelper.IsValidEmail(email)) {
            if (!GetAllUsers().Any(u => u.Email == email)) {
                string username = email.Split('@')[0];
                var AddUser = "insert into \"HowlDev.User\" (email, displayName, role) values (@email, @username, 0)";
                conn.Execute(AddUser, new { email, username });
            }
            return await NewSignIn(email);
        } else {
            return "Invalid email.";
        }
    }

    /// <summary>
    /// Adds a new line to the API key table and adds a line to the validation table for email validation.
    /// </summary>
    /// <returns>API key the user should use for login</returns>
    public async Task<string> NewSignIn(string email) {
        if (CurrentUnauthorizedKeys(email) < 2) {
            string newApiKey = StringHelper.GenerateRandomString(20);
            string validationToken = StringHelper.GenerateRandomString(40);
            await service.SendValidationEmail(email, validationToken);

            var addValidation = "insert into \"HowlDev.Key\" (email, apiKey, validatorToken) values (@email, @newApiKey, @validationToken)";
            conn.Execute(addValidation, new { email, newApiKey, validationToken });

            return newApiKey;
        } else {
            return "Too many unauthorized keys";
        }
    }

    /// <summary>
    /// <c>For Debug Only</c>, I wouldn't reccommend assigning this an endpoint. Returns all users sorted by 
    /// email. 
    /// </summary>
    public IEnumerable<EmailAccount> GetAllUsers() {
        var GetUsers = "select p.email, p.displayName from \"HowlDev.User\" p order by 1 asc";
        return conn.Query<EmailAccount>(GetUsers);
    }

    /// <summary>
    /// Returns the user object from the given email.
    /// </summary>
    public EmailAccount GetUser(string email) {
        var GetUsers = "select p.email, p.displayName, p.role from \"HowlDev.User\" p where email = @email";
        return conn.QuerySingle<EmailAccount>(GetUsers, new { email });
    }

    /// <summary>
    /// You can decide whether or not the returned date is valid (if you want expiration dates). 
    /// </summary>
    /// <param name="email">Email address used</param>
    /// <param name="key">API Key</param>
    /// <returns>Null or DateTime</returns>
    public DateTime? IsValidApiKey(string email, string key) {
        var validKey = "select k.validatedon from \"HowlDev.Key\" k where email = @email and apiKey = @key";
        return conn.QuerySingle<DateTime?>(validKey, new { email, key });
    }

    /// <summary>
    /// Validates with a token and email. Does not return anything. 
    /// </summary>
    public void Validate(string email, string token) {
        string time = DateTime.Now.ToUniversalTime().ToString("u");
        var validate = $"update \"HowlDev.Key\" hdk set validatedon = '{time}' where email = @email and validatortoken = @token";
        conn.Execute(validate, new { email, token });
    }

    /// <summary>
    /// Updates the api key with the current DateTime value. This allows recently signed-in users to 
    /// continue being signed in on their key. 
    /// </summary>
    public void ReValidate(string email, string key) {
        string time = DateTime.Now.ToUniversalTime().ToString("u");
        var validate = $"update \"HowlDev.Key\" hdk set validatedon = '{time}' where email = @email and apiKey = @key";
        conn.Execute(validate, new { email, key });
    }

    /// <summary>
    /// Deletes all sign-in records by the user and their place in the User table.
    /// </summary>
    public void DeleteUser(string email) {
        GlobalSignOut(email);

        var removeUser = "delete from \"HowlDev.User\" where email = @email";
        conn.Execute(removeUser, new { email });
    }

    /// <summary>
    /// Signs a user out globally, such as in the instance of someone else gaining access to their account.
    /// </summary>
    public void GlobalSignOut(string email) {
        var removeKeys = "delete from \"HowlDev.Key\" where email = @email";
        conn.Execute(removeKeys, new { email });
    }

    /// <summary>
    /// Sign out on an individual device by passing the key you want signed out. 
    /// </summary>
    public void KeySignOut(string email, string key) {
        var removeKey = "delete from \"HowlDev.Key\" where email = @email and apiKey = @key";
        conn.Execute(removeKey, new { email, key });
    }

    private int CurrentUnauthorizedKeys(string email) {
        var countKeys = "select count(*) from \"HowlDev.Key\" where email = @email and validatedon is null";
        return conn.QuerySingle<int>(countKeys, new { email });
    }
}