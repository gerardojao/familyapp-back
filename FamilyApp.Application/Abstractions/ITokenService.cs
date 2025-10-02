namespace FamilyApp.Application.Abstractions;

using FamilyApp.Domain.Entities;

public interface ITokenService
{
    string CreateToken(AppUser user, Guid jti, out DateTime expiresAt);
}
