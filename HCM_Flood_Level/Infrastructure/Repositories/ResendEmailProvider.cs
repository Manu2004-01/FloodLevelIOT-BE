using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repositories
{
    public class ResendEmailProvider : IEmailProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ResendEmailProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            var apiKey = _configuration["Resend:ApiKey"] ?? Environment.GetEnvironmentVariable("RESEND_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Missing Resend API key (Resend:ApiKey or RESEND_API_KEY).");

            var fromEmail = _configuration["Resend:FromEmail"] ?? _configuration["Email:FromEmail"];
            if (string.IsNullOrWhiteSpace(fromEmail))
                throw new InvalidOperationException("Missing sender email (Resend:FromEmail or Email:FromEmail).");

            var fromName = _configuration["Resend:FromName"] ?? _configuration["Email:FromName"] ?? "Flood Level HCM";
            var from = $"{fromName} <{fromEmail}>";

            var payload = new
            {
                from,
                to = new[] { toEmail },
                subject,
                text = body
            };

            var client = _httpClientFactory.CreateClient("Resend");
            client.Timeout = TimeSpan.FromSeconds(20);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync("https://api.resend.com/emails", content, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Resend send failed ({(int)response.StatusCode}): {Truncate(responseText, 500)}");
            }
        }

        private static string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return "(empty)";
            return value.Length <= maxLength ? value : value[..maxLength] + "...";
        }
    }
}
