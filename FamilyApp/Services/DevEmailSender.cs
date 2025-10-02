// Services/DevEmailSender.cs
namespace FamilyApp.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _cfg;
        public SmtpEmailSender(IConfiguration cfg) => _cfg = cfg;
        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var host = _cfg["Smtp:Host"] ?? "smtp.resend.com";
            var port = int.TryParse(_cfg["Smtp:Port"], out var p) ? p : 587;
            var user = _cfg["Smtp:User"] ?? "resend";
            var pass = _cfg["Smtp:Pass"];               
            var from = _cfg["Smtp:From"] ?? "no-reply@familyapp.store";
            var html = $@"
              <p>Hola{(string.IsNullOrWhiteSpace(user) ? "" : $" ")},</p>
              <p>Has solicitado restablecer tu contraseña. Haz clic en el siguiente enlace:</p>
              <p><a href="""">Restablecer contraseña</a></p>
              <p>El enlace caduca en 30 minutos. Si no lo solicitaste, ignora este correo.</p>";

            using var client = new System.Net.Mail.SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential(user, pass)
            };

            using var msg = new System.Net.Mail.MailMessage(from, to)
            {
                Subject = subject,
                Body = html,
                IsBodyHtml = true
            };

            await client.SendMailAsync(msg);
        }
    }
}
