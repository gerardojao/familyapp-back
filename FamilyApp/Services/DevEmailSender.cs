// Services/DevEmailSender.cs
namespace FamilyApp.Services
{
    public class DevEmailSender : IEmailSender
    {
        public Task SendAsync(string to, string subject, string htmlBody)
        {
            Console.WriteLine("=== DEV EMAIL ===");
            Console.WriteLine($"To: {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine(htmlBody);
            Console.WriteLine("=================");
            return Task.CompletedTask;
        }
    }
}
