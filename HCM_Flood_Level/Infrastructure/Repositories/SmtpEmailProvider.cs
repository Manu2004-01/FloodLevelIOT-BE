using Core.Interfaces;
using MailKit.Security;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class SmtpEmailProvider : IEmailProvider
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            var host = _configuration["Email:SmtpHost"] ?? _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            var portStr = _configuration["Email:SmtpPort"];
            var port = int.TryParse(portStr, out var p) ? p : 587;
            var user = _configuration["Email:UserName"];
            var password = _configuration["Email:Password"];
            var fromEmail = _configuration["Email:FromEmail"] ?? user;
            var fromName = _configuration["Email:FromName"] ?? "Flood Level HCM";
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
                throw new InvalidOperationException("Email:UserName and Email:Password must be set in configuration.");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };
            using var client = new SmtpClient();
            client.Timeout = 20000; // 20s to avoid hanging requests on cloud SMTP failures

            try
            {
                await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, cancellationToken);
            }
            catch (Exception ex) when (port == 587)
            {
                // Some platforms/networks block 587 STARTTLS. Fallback to 465 SSL.
                try
                {
                    if (client.IsConnected)
                        await client.DisconnectAsync(true, cancellationToken);
                    await client.ConnectAsync(host, 465, SecureSocketOptions.SslOnConnect, cancellationToken);
                }
                catch
                {
                    throw new InvalidOperationException(
                        $"SMTP connect failed to {host}:587 and fallback {host}:465. {ex.Message}", ex);
                }
            }

            await client.AuthenticateAsync(user, password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}
