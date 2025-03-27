
namespace EmailAuthenticator;

public class ValidPaths : IValidPaths {
    public List<string> Paths => new List<string>() {"/api/users" };
}
