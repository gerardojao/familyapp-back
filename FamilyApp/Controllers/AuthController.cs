// Controllers/AuthController.cs
using FamilyApp.Data;
using FamilyApp.DTOs.LoginDTO;
using FamilyApp.Models;
using FamilyApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly dbContext _db;
    private readonly IPasswordService _pwd;
    private readonly ITokenService _tokens;

    public AuthController(dbContext db, IPasswordService pwd, ITokenService tokens)
    {
        _db = db; _pwd = pwd; _tokens = tokens;
    }

    public class LoginDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        // ✅ primero el hash guardado, luego el password en texto plano
        if (user == null || !_pwd.Verify(user.PasswordHash, dto.Password) || !user.IsActive)
            return Unauthorized(new { message = "Credenciales inválidas" });

        var jti = Guid.NewGuid();
        var token = _tokens.CreateToken(user, jti, out var expiresAt);

        user.ActiveSessionJti = jti;
        user.ActiveSessionExpiresAt = expiresAt;
        await _db.SaveChangesAsync();

        return Ok(new { token, expiresAt, user = new { user.Id, user.Email, user.Role } });
    }


    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return Unauthorized();

        user.ActiveSessionJti = null;
        user.ActiveSessionExpiresAt = null;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Sesión cerrada." });
    }


    // POST: api/auth/register
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        // Validaciones mínimas
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Email y contraseña son obligatorios." });

        var email = dto.Email.Trim().ToLowerInvariant();

        // Reglas simples de contraseña (ajústalas a tu gusto)
        if (dto.Password.Length < 8 ||
            !dto.Password.Any(char.IsUpper) ||
            !dto.Password.Any(char.IsLower) ||
            !dto.Password.Any(char.IsDigit))
        {
            return BadRequest(new { message = "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número." });
        }

        // ¿ya existe?
        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        if (exists) return Conflict(new { message = "El correo ya está registrado." });

        // Crear usuario (ROL FIJO user)
        var user = new AppUser
        {
            Email = email,
            FullName = dto.FullName,
            PasswordHash = _pwd.Hash(dto.Password),
            Role = "user",               // <- forzado en servidor
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Opcional: auto-login tras registro (misma lógica que Login)
        var jti = Guid.NewGuid();
        var token = _tokens.CreateToken(user, jti, out var expiresAt);
        user.ActiveSessionJti = jti;
        user.ActiveSessionExpiresAt = expiresAt;
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Register), new
        {
            user = new { user.Id, user.Email, user.Role, user.FullName },
            token,
            expiresAt
        });
    }
}
