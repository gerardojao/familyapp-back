﻿using System.Threading.Tasks;

namespace FamilyApp.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }
}
