namespace EmailAuthenticator;

public interface IEmailService {
    Task SendValidationEmail(string email, string validationToken);
}
