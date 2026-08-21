// <copyright file="TwilioSmsService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using HotshotLogistics.Contracts.Services;
using HotshotLogistics.Domain.DTOs;
using HotshotLogistics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HotshotLogistics.Data.Services
{
    /// <summary>
    ///     Service for sending SMS messages via Twilio.
    /// </summary>
    public class TwilioSmsService : ICommunicationService
    {
        private readonly ILogger<TwilioSmsService> _logger;
        private readonly TwilioSettings _settings;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TwilioSmsService" /> class.
        /// </summary>
        /// <param name="settings">The Twilio settings.</param>
        /// <param name="logger">The logger.</param>
        public TwilioSmsService(
            IOptions<TwilioSettings> settings,
            ILogger<TwilioSmsService> logger)
        {
            ArgumentNullException.ThrowIfNull(settings);
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public string Type => "Sms";

        /// <inheritdoc />
        public async Task<bool> SendAsync(CommunicationMessage message, CancellationToken cancellationToken = default)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (string.IsNullOrWhiteSpace(message.To))
            {
                throw new ArgumentException("Recipient phone number cannot be empty", nameof(message));
            }

            if (string.IsNullOrWhiteSpace(message.Body))
            {
                throw new ArgumentException("Message body cannot be empty", nameof(message));
            }

            try
            {
                // Format message with template data
                string formattedBody = FormatMessage(message.Body, message.TemplateData);

                // Initialize Twilio client
                TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);

                CreateMessageOptions messageOptions = new(new PhoneNumber(message.To))
                {
                    From = new PhoneNumber(_settings.FromPhoneNumber),
                    Body = formattedBody
                };

                MessageResource result = await MessageResource.CreateAsync(messageOptions);

                _logger.LogInformation("SMS sent successfully to {To}. SID: {Sid}", message.To, result.Sid);

                return result.Status == MessageResource.StatusEnum.Sent ||
                       result.Status == MessageResource.StatusEnum.Queued ||
                       result.Status == MessageResource.StatusEnum.Delivered;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {To}", message.To);
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
