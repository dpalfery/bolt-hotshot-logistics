// <copyright file="JobDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Contracts.Models
{
    using System;
    using HotshotLogistics.Contracts.Models;

    /// <summary>
    /// Data transfer object for job information.
    /// </summary>
    public class JobDto : IJob
    {
        /// <summary>
        /// Gets or sets the unique identifier for the job.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the title of the job.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the pickup address for the job.
        /// </summary>
        public string PickupAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the dropoff address for the job.
        /// </summary>
        public string DropoffAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current status of the job.
        /// </summary>
        public JobStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the priority level of the job.
        /// </summary>
        public JobPriority Priority { get; set; }

        /// <summary>
        /// Gets or sets the monetary amount for the job.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the estimated delivery time for the job.
        /// </summary>
        public string EstimatedDeliveryTimeString { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the assigned driver.
        /// </summary>
        public int? AssignedDriverId { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp of the job.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the last update timestamp of the job.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the scheduled pickup time for the job.
        /// </summary>
        public DateTime ScheduledPickupTime { get; set; }

        /// <summary>
        /// Gets or sets the special instructions for the job.
        /// </summary>
        public string SpecialInstructions { get; set; } = string.Empty;

        // IJob interface implementation
        public string CustomerId { get; set; } = string.Empty;
        public Location PickupLocation { get; set; } = new Location();
        public Location DeliveryLocation { get; set; } = new Location();
        public CargoDetails Cargo { get; set; } = new CargoDetails();
        public PricingDetails Pricing { get; set; } = new PricingDetails();
        public DateTime EstimatedDeliveryTime
        {
            get
            {
                if (DateTime.TryParse(this.EstimatedDeliveryTimeString, out var result))
                {
                    return result;
                }
                return DateTime.MinValue;
            }
            set => this.EstimatedDeliveryTimeString = value.ToString("O");
        }
        public DateTime? ActualPickupTime { get; set; }
        public DateTime? ActualDeliveryTime { get; set; }
        public List<JobDocument> Documents { get; set; } = new List<JobDocument>();
        public TrackingInfo Tracking { get; set; } = new TrackingInfo();
    }
}
