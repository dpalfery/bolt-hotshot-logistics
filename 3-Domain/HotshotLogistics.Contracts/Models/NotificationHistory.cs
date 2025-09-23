// <copyright file="NotificationHistory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Contracts.Models
{
    using System;

    /// <summary>
    /// Represents a notification history record.
    /// </summary>
    public class NotificationHistory
    {
        /// <summary>
        /// Gets or sets the unique identifier for the notification history record.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user identifier who received the notification.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of notification.
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Gets or sets the notification title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the notification message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the notification was sent.
        /// </summary>
        public DateTime SentAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the notification was sent successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the delivery status of the notification.
        /// </summary>
        public NotificationDeliveryStatus DeliveryStatus { get; set; } = NotificationDeliveryStatus.Sent;

        /// <summary>
        /// Gets or sets the timestamp when the notification was delivered (if applicable).
        /// </summary>
        public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the notification was read (if applicable).
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Gets or sets the channels used to send the notification.
        /// </summary>
        public string Channels { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets any error message if the notification failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the number of retry attempts made.
        /// </summary>
        public int RetryAttempts { get; set; }

        /// <summary>
        /// Gets a formatted display string for the notification.
        /// </summary>
        /// <returns>A formatted string representation.</returns>
        public string GetDisplayString()
        {
            var status = Success ? "✓" : "✗";
            return $"{status} {Type} - {Title} ({SentAt:MM/dd/yyyy HH:mm})";
        }

        /// <summary>
        /// Gets the age of the notification.
        /// </summary>
        /// <returns>The time elapsed since the notification was sent.</returns>
        public TimeSpan GetAge()
        {
            return DateTime.UtcNow - SentAt;
        }

        /// <summary>
        /// Checks if the notification is recent (within the last hour).
        /// </summary>
        /// <returns>True if the notification is recent, false otherwise.</returns>
        public bool IsRecent()
        {
            return GetAge() <= TimeSpan.FromHours(1);
        }
    }


    /// <summary>
    /// Represents the delivery status of a notification.
    /// </summary>
    public enum NotificationDeliveryStatus
    {
        /// <summary>
        /// Notification is pending delivery.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Notification has been sent.
        /// </summary>
        Sent = 1,

        /// <summary>
        /// Notification has been delivered.
        /// </summary>
        Delivered = 2,

        /// <summary>
        /// Notification has been read.
        /// </summary>
        Read = 3,

        /// <summary>
        /// Notification delivery failed.
        /// </summary>
        Failed = 4,

        /// <summary>
        /// Notification was bounced (email) or rejected.
        /// </summary>
        Bounced = 5
    }
}