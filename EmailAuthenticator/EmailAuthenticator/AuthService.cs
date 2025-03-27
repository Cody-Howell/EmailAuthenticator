
using Dapper;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace EmailAuthenticator;

public class AuthService(IDbConnection conn) {
    public string AddUser(string email) {
        if (StringHelper.IsValidEmail(email)) {
            if (!GetAllUsers().Any(u => u.Email == email)) {
                string username = email.Split('@')[0];
                var AddUser = "insert into \"HowlDev.User\" (email, displayName) values (@email, @username)";
                conn.Execute(AddUser, new { email, username });
            }
            return NewSignIn(email);
        } else {
            return "Invalid email.";
        }
    }

    /// <summary>
    /// Adds a new line to the API key table and adds a line to the validation table for email validation.
    /// </summary>
    /// <returns>API key the user should use for login</returns>
    public string NewSignIn(string email) {
        string newApiKey = StringHelper.GenerateRandomString(20);

        return newApiKey;
    }

    public IEnumerable<User> GetAllUsers() {
        var GetUsers = "select p.email, p.displayName from \"HowlDev.User\" p order by 1 asc";
        return conn.Query<User>(GetUsers);
    }
}