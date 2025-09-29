// <copyright file="IJob.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Contracts.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a job in the logistics system.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Gets or sets the unique identifier for the job.
        /// </summary>
        string Id { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier for the job.
        /// </summary>
        string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the title of the job.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the pickup location for the job.
        /// </summary>
        Location PickupLocation { get; set; }

        /// <summary>
        /// Gets or sets the delivery location for the job.
        /// </summary>
        Location DeliveryLocation { get; set; }

        /// <summary>
        /// Gets or sets the cargo details for the job.
        /// </summary>
        CargoDetails Cargo { get; set; }

        /// <summary>
        /// Gets or sets the current status of the job.
        /// </summary>
        JobStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the priority level of the job.
        /// </summary>
        JobPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets the pricing details for the job.
        /// </summary>
        PricingDetails Pricing { get; set; }

        /// <summary>
        /// Gets or sets the scheduled pickup time for the job.
        /// </summary>
        DateTime ScheduledPickupTime { get; set; }

        /// <summary>
        /// Gets or sets the estimated delivery time for the job.
        /// </summary>
        DateTime EstimatedDeliveryTime { get; set; }

        /// <summary>
        /// Gets or sets the actual pickup time for the job.
        /// </summary>
        DateTime? ActualPickupTime { get; set; }

        /// <summary>
        /// Gets or sets the actual delivery time for the job.
        /// </summary>
        DateTime? ActualDeliveryTime { get; set; }

        /// <summary>
        /// Gets or sets the special instructions for the job.
        /// </summary>
        string SpecialInstructions { get; set; }

        /// <summary>
        /// Gets or sets the ID of the assigned driver.
        /// </summary>
        int? AssignedDriverId { get; set; }

        /// <summary>
        /// Gets or sets the list of documents associated with the job.
        /// </summary>
        List<JobDocument> Documents { get; set; }

        /// <summary>
        /// Gets or sets the tracking information for the job.
        /// </summary>
        TrackingInfo Tracking { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp of the job.
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp of the job.
        /// </summary>
        DateTime? UpdatedAt { get; set; }
    }


}
