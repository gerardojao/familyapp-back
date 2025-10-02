using System.Security.Cryptography;
using System.Text;

namespace FamilyApp.Application.Services;

public static class SecureToken
{
    public static string CreateUrlToken(int bytesLen = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(bytesLen);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", string.Empty);
    }

    public static string Sha256Base64(string text)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
        return Convert.ToBase64String(hash);
    }
}
