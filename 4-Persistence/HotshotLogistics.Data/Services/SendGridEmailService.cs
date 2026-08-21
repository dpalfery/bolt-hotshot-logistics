// <copyright file="SendGridEmailService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Net;
using HotshotLogistics.Contracts.Services;
using HotshotLogistics.Domain.DTOs;
using HotshotLogistics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace HotshotLogistics.Data.Services
{
    /// <summary>
    ///     Service for sending emails via SendGrid.
    /// </summary>
    public class SendGridEmailService : ICommunicationService
    {
        private readonly ILogger<SendGridEmailService> _logger;
        private readonly SendGridSettings _settings;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SendGridEmailService" /> class.
        /// </summary>
        /// <param name="settings">The SendGrid settings.</param>
        /// <param name="logger">The logger.</param>
        public SendGridEmailService(
            IOptions<SendGridSettings> settings,
            ILogger<SendGridEmailService> logger)
        {
            ArgumentNullException.ThrowIfNull(settings);
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public string Type => "Email";

        /// <inheritdoc />
        public async Task<bool> SendAsync(CommunicationMessage message, CancellationToken cancellationToken = default)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (string.IsNullOrWhiteSpace(message.To))
            {
                throw new ArgumentException("Recipient email address cannot be empty", nameof(message));
            }

            if (string.IsNullOrWhiteSpace(message.Body))
            {
                throw new ArgumentException("Message body cannot be empty", nameof(message));
            }

            try
            {
                // Format message with template data
                string formattedBody = FormatMessage(message.Body, message.TemplateData);

                SendGridClient client = new(_settings.ApiKey);
                EmailAddress from = new(_settings.FromEmail, _settings.FromName);
                EmailAddress to = new(message.To);
                string subject = message.Subject ?? "Notification";
                string plainTextContent = formattedBody;
                string htmlContent = $"<p>{formattedBody.Replace("\n", "<br>")}</p>"; // Simple HTML conversion

                SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                Response response = await client.SendEmailAsync(msg, cancellationToken);

                _logger.LogInformation("Email sent successfully to {To}. Status: {Status}", message.To,
                    response.StatusCode);

                return response.StatusCode == HttpStatusCode.Accepted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", message.To);
                return false;
            }
        }

        /// <summary>
        ///     Formats a message template with the provided data.
        /// </summary>
        /// <param name="template">The message template.</param>
        /// <param name="data">The template data.</param>
        /// <returns>The formatted message.</returns>
        private static string FormatMessage(string template, Dictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(template) || data.Count == 0)
            {
                return template;
            }

            string result = template;
            foreach (KeyValuePair<string, string> kvp in data)
            {
                result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
            }

            return result;
        }
    }
}
