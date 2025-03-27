using System.Security.Cryptography;
using System.Text;

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
}
