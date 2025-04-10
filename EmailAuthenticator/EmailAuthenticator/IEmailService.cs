namespace EmailAuthenticator;

/// <summary>
/// Used by the AuthService to send validation emails to validate API keys. 
/// </summary>
public interface IEmailService {
    /// <summary>
    /// Taking in an email and token, you must return a link that gets them to the endpoint 
    /// that validates through the AuthService validator. This can be done in development through 
    /// a Console.WriteLine call, but in production (keeping with the name) should send an email. 
    /// </summary>
    Task SendValidationEmail(string email, string validationToken);
}
