// <copyright file="ILocationTracking.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Contracts.Models
{
    using System;

    /// <summary>
    /// Interface for location tracking records.
    /// </summary>
    public interface ILocationTracking
    {
        /// <summary>
        /// Gets or sets the unique identifier for the location tracking record.
        /// </summary>
        long Id { get; set; }

        /// <summary>
        /// Gets or sets the job identifier this tracking record belongs to.
        /// </summary>
        string JobId { get; set; }

        /// <summary>
        /// Gets or sets the driver identifier for this tracking record.
        /// </summary>
        int DriverId { get; set; }

        /// <summary>
        /// Gets or sets the latitude coordinate.
        /// </summary>
        decimal Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude coordinate.
        /// </summary>
        decimal Longitude { get; set; }

        /// <summary>
        /// Gets or sets the speed in miles per hour.
        /// </summary>
        decimal? Speed { get; set; }

        /// <summary>
        /// Gets or sets the heading/direction in degrees (0-359).
        /// </summary>
        int? Heading { get; set; }

        /// <summary>
        /// Gets or sets the GPS accuracy in meters.
        /// </summary>
        decimal? Accuracy { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this location was recorded.
        /// </summary>
        DateTime Timestamp { get; set; }
    }
}
