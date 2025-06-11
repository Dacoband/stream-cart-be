using Microsoft.Extensions.Options;
using Shared.Common.Models;
using Shared.Common.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shared.Common.Services.Email
{
    public class MailJetEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly HttpClient _httpClient;
        private const string MailJetApiUrl = "https://api.mailjet.com/v3.1/send";

        public MailJetEmailService(IOptions<EmailSettings> emailSettings, HttpClient httpClient)
        {
            _emailSettings = emailSettings.Value ?? throw new ArgumentNullException(nameof(emailSettings));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            // Configure HttpClient for MailJet
            var authenticationString = $"{_emailSettings.ApiKey}:{_emailSettings.SecretKey}";
            var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody, string? toName = null)
        {
            var recipients = new[] { new EmailRecipient(to, toName) };
            return await SendEmailInternalAsync(recipients, subject, htmlBody);
        }

        public async Task<bool> SendEmailAsync(string[] to, string subject, string htmlBody, string[]? toNames = null)
        {
            var recipients = new List<EmailRecipient>();

            for (int i = 0; i < to.Length; i++)
            {
                string? name = toNames != null && i < toNames.Length ? toNames[i] : null;
                recipients.Add(new EmailRecipient(to[i], name));
            }

            return await SendEmailInternalAsync(recipients, subject, htmlBody);
        }

        public async Task<bool> SendTemplateEmailAsync(string to, string templateId, object templateData, string? toName = null)
        {
            // In a real implementation, this would use MailJet's template features
            // For now, we'll just simulate it
            var htmlBody = $"This would be rendered using template {templateId} with the provided data.";
            return await SendEmailAsync(to, "Template Email", htmlBody, toName);
        }

        private async Task<bool> SendEmailInternalAsync(IEnumerable<EmailRecipient> recipients, string subject, string htmlBody)
        {
            try
            {
                var message = new
                {
                    Messages = new[]
                    {
                        new
                        {
                            From = new
                            {
                                Email = _emailSettings.DefaultFromEmail,
                                Name = _emailSettings.DefaultFromName
                            },
                            To = recipients.Select(r => new
                            {
                                Email = r.Email,
                                Name = r.Name ?? "Recipient"
                            }).ToArray(),
                            Subject = subject,
                            HTMLPart = htmlBody
                        }
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(message),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(MailJetApiUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"MailJet API Response: {responseBody}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }
}
