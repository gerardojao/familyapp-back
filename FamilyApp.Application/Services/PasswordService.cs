using FamilyApp.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace FamilyApp.Application.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<string> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(string.Empty, password);

    public bool Verify(string hash, string password)
        => _hasher.VerifyHashedPassword(string.Empty, hash, password) is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}
