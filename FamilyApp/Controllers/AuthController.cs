﻿// Controllers/AuthController.cs
using FamilyApp.Data;
using FamilyApp.DTOs.Auth;
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
    private readonly IEmailSender _mailer;
    private readonly IConfiguration _cfg;
    private readonly IWebHostEnvironment _env;

    public AuthController(
        dbContext db,
        IPasswordService pwd,
        ITokenService tokens,
        IEmailSender mailer,
        IConfiguration cfg,
        IWebHostEnvironment env)
    {
        _db = db;
        _pwd = pwd;
        _tokens = tokens;
        _mailer = mailer;
        _cfg = cfg;
        _env = env;
    }

    public class LoginDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

        [Authorize]
        [HttpGet("auth/whoami")]
        public IActionResult WhoAmI() => Ok(new
        {
            sub = User.FindFirst("sub")?.Value,
            email = User.FindFirst("email")?.Value,
            jti = User.FindFirst("jti")?.Value
        });


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
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        if (dto is null)
            return BadRequest(new { ok = false, message = "Body vacío." });

        var email = dto.Email?.Trim();
        var password = dto.Password ?? "";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return BadRequest(new { ok = false, message = "Email y contraseña son obligatorios." });

        email = email.ToLowerInvariant();

        // Reglas: ajústalas a lo que quieras exigir
        if (password.Length < 8 ||
            !password.Any(char.IsUpper) ||
            !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit))
        {
            return BadRequest(new
            {
                ok = false,
                message = "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número."
            });
        }

        // ¿Existe ya?
        var exists = await _db.Users.AsNoTracking().AnyAsync(u => u.Email == email);
        if (exists)
            return Conflict(new { ok = false, message = "El correo ya está registrado." });

        // Crear usuario
        var user = new AppUser
        {
            Email = email,
            FullName = dto.FullName, // <- ahora sí llega desde "nombre"
            PasswordHash = _pwd.Hash(password),
            Role = "user",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(); // aquí ya tienes user.Id

        // (Opcional) Auto-login tras registro
        var jti = Guid.NewGuid();
        var token = _tokens.CreateToken(user, jti, out var expiresAt);
        user.ActiveSessionJti = jti;
        user.ActiveSessionExpiresAt = expiresAt;
        await _db.SaveChangesAsync();

        // Devuelve shape que tu front ya sabe leer
        return Ok(new
        {
            ok = true,
            message = "Cuenta creada correctamente.",
            data = new
            {
                user = new { user.Id, user.Email, user.Role, user.FullName },
                token,
                expiresAt
            }
        });
    }



    // === FORGOT PASSWORD ===
    [AllowAnonymous]
    [HttpPost("forgot")]
    public async Task<IActionResult> Forgot([FromBody] ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return Ok(new { message = "Si el email existe, se ha enviado un enlace de reseteo." });

        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        if (user == null)
            return Ok(new { message = "Si el email existe, se ha enviado un enlace de reseteo." });

        var now = DateTime.UtcNow;

        // Invalidar solicitudes previas pendientes para evitar enlaces viejos reutilizables
        await _db.PasswordResets
            .Where(x => x.UserId == user.Id && x.UsedAt == null)
            .ExecuteDeleteAsync();

        // Generar token y guardar hash
        var token = SecureToken.CreateUrlToken(32);
        var tokenHash = SecureToken.Sha256Base64(token);

        _db.PasswordResets.Add(new PasswordReset
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(30),
            RequestIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            RequestUserAgent = Request.Headers.UserAgent.ToString()
        });
        await _db.SaveChangesAsync();

        // Construir link usando _cfg (NO builder)
        var frontBase = _cfg["App:FrontendBaseUrl"];
        var link = !string.IsNullOrWhiteSpace(frontBase)
            ? $"{frontBase}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}"
            : $"{Request.Scheme}://{Request.Host}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";

        var html = $@"
            <p>Hola,</p>
            <p>Has solicitado restablecer tu contraseña. Haz clic en el siguiente enlace:</p>
            <p><a href=\"{link}\">Restablecer contraseña</a></p>
            <p>El enlace caduca en 30 minutos. Si no lo solicitaste, ignora este correo.</p>";

        await _mailer.SendAsync(user.Email, "Restablecer contraseña - FamilyApp", html);

        if (_env.IsDevelopment())
            return Ok(new { message = "Email enviado (DEV).", devToken = token, devLink = link });

        return Ok(new { message = "Si el email existe, se ha enviado un enlace de reseteo." });
    }



[AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest(new { message = "Datos incompletos." });

        // Reglas de contraseña (ajusta a tus políticas)
        if (dto.NewPassword.Length < 8 ||
            !dto.NewPassword.Any(char.IsUpper) ||
            !dto.NewPassword.Any(char.IsLower) ||
            !dto.NewPassword.Any(char.IsDigit))
        {
            return BadRequest(new { message = "La nueva contraseña no cumple los requisitos." });
        }

        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        if (user == null) return BadRequest(new { message = "Token inválido o caducado." });

        var tokenHash = SecureToken.Sha256Base64(dto.Token);
        var pr = await _db.PasswordResets
            .Where(x => x.UserId == user.Id && x.TokenHash == tokenHash)
            .FirstOrDefaultAsync();

        if (pr == null || pr.UsedAt != null || pr.ExpiresAt < DateTime.UtcNow)
            return BadRequest(new { message = "Token inválido o caducado." });

        // Actualizamos contraseña
        user.PasswordHash = _pwd.Hash(dto.NewPassword);

        // Forzamos cierre de sesión de todos los dispositivos
        user.ActiveSessionJti = null;
        user.ActiveSessionExpiresAt = null;

        pr.UsedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Contraseña actualizada. Vuelve a iniciar sesión." });
    }
}
