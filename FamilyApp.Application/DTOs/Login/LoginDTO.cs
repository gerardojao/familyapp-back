namespace FamilyApp.Application.DTOs.Login
{
    public class LoginDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public bool Force { get; set; } = false; // si true, reemplaza sesión previa
    }

    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = "";
        public DateTime ExpiresAt { get; set; }
        public string? Role { get; set; }
        public string? Email { get; set; }
    }
}
