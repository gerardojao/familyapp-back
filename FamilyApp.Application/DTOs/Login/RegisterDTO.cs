using System.Text.Json.Serialization;

namespace FamilyApp.Application.DTOs.Login
{
    public class RegisterDTO
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        // El front envía "nombre"
        [JsonPropertyName("nombre")]
        public string? FullName { get; set; }
    }
}
