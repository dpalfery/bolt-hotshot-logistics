// <copyright file="AzureNotificationHubService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using HotshotLogistics.Contracts.Services;
using HotshotLogistics.Domain.ValueObjects;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationHubSettings = HotshotLogistics.Domain.ValueObjects.NotificationHubSettings;

namespace HotshotLogistics.Data.Services
{
    /// <summary>
    ///     Service for sending push notifications via Azure Notification Hubs.
    /// </summary>
    public class AzureNotificationHubService : ICommunicationService
    {
        private readonly ILogger<AzureNotificationHubService> _logger;
        private readonly NotificationHubSettings _settings;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AzureNotificationHubService" /> class.
        /// </summary>
        /// <param name="settings">The Notification Hub settings.</param>
        /// <param name="logger">The logger.</param>
        public AzureNotificationHubService(
            IOptions<NotificationHubSettings> settings,
            ILogger<AzureNotificationHubService> logger)
        {
            ArgumentNullException.ThrowIfNull(settings);
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public string Type => "Push";

        /// <inheritdoc />
        public async Task<bool> SendAsync(CommunicationMessage message, CancellationToken cancellationToken = default)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (string.IsNullOrWhiteSpace(message.To))
            {
                throw new ArgumentException("Device token/tag cannot be empty", nameof(message));
            }

            if (string.IsNullOrWhiteSpace(message.Body))
            {
                throw new ArgumentException("Message body cannot be empty", nameof(message));
            }

            try
            {
                // Format message with template data
                string formattedBody = FormatMessage(message.Body, message.TemplateData);

                NotificationHubClient hub = NotificationHubClient.CreateClientFromConnectionString(
                    _settings.ConnectionString,
                    _settings.HubName);

                // Send FCM notification (assuming Android devices)
                // Using the 'To' field as a tag for targeting specific devices
                NotificationOutcome outcome =
                    await hub.SendFcmNativeNotificationAsync(formattedBody, message.To, cancellationToken);

                _logger.LogInformation("Push notification sent to {To}. Outcome: {Outcome}", message.To, outcome.State);

                return outcome.State == NotificationOutcomeState.Enqueued ||
                       outcome.State == NotificationOutcomeState.Completed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send push notification to {To}", message.To);
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
