using FamilyApp.Application.Abstractions;

namespace FamilyApp.Infrastructure.Email;

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
