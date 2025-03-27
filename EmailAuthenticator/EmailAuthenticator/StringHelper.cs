using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace EmailAuthenticator;

public static class StringHelper {
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()-_=+";

    public static string CustomHash(string s) {
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(s)));
    }

    public static string GenerateRandomString(int length = 12) {
        var password = new StringBuilder();
        using var rng = RandomNumberGenerator.Create();
        var buffer = new byte[length];

        rng.GetBytes(buffer);

        for (int i = 0; i < length; i++) {
            var index = buffer[i] % Chars.Length;
            password.Append(Chars[index]);
        }

        return password.ToString();
    }

    /// <summary>
    /// Retrieved from the Microsoft documentation, just straight copy-pasted. Pseudo-validates
    /// an email using Regex.
    /// </summary>
    public static bool IsValidEmail(string email) {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                  RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match) {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                string domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        } catch (RegexMatchTimeoutException e) {
            return false;
        } catch (ArgumentException e) {
            return false;
        }

        try {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        } catch (RegexMatchTimeoutException) {
            return false;
        }
    }
}
