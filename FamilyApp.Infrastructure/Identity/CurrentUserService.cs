using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FamilyApp.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace FamilyApp.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http) => _http = http;

    public string? UserIdOrEmail =>
        _http.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _http.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => _http.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public int? UserIdInt
    {
        get
        {
            var s = _http.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(s, out var id) ? id : null;
        }
    }
}
