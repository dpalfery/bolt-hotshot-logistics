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
        /// Gets or sets the special instructions for the job.
        /// </summary>
        string SpecialInstructions { get; set; }

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

    /// <summary>
    /// Represents a physical location with coordinates.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Gets or sets the address of the location.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the latitude coordinate.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude coordinate.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the contact name at the location.
        /// </summary>
        public string ContactName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the contact phone number at the location.
        /// </summary>
        public string ContactPhone { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents cargo details for a job.
    /// </summary>
    public class CargoDetails
    {
        /// <summary>
        /// Gets or sets the description of the cargo.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the weight of the cargo in pounds.
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// Gets or sets the value of the cargo for insurance purposes.
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// Gets or sets the dimensions of the cargo.
        /// </summary>
        public Dimensions Dimensions { get; set; } = new Dimensions();

        /// <summary>
        /// Gets or sets a value indicating whether the cargo requires special handling.
        /// </summary>
        public bool RequiresSpecialHandling { get; set; }

        /// <summary>
        /// Gets or sets the special handling instructions.
        /// </summary>
        public string SpecialHandlingInstructions { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the dimensions of cargo.
    /// </summary>
    public class Dimensions
    {
        /// <summary>
        /// Gets or sets the length in inches.
        /// </summary>
        public decimal Length { get; set; }

        /// <summary>
        /// Gets or sets the width in inches.
        /// </summary>
        public decimal Width { get; set; }

        /// <summary>
        /// Gets or sets the height in inches.
        /// </summary>
        public decimal Height { get; set; }

        /// <summary>
        /// Gets the volume in cubic feet.
        /// </summary>
        public decimal Volume => (Length * Width * Height) / 1728; // Convert cubic inches to cubic feet
    }

    /// <summary>
    /// Represents pricing details for a job.
    /// </summary>
    public class PricingDetails
    {
        /// <summary>
        /// Gets or sets the base rate for the job.
        /// </summary>
        public decimal BaseRate { get; set; }

        /// <summary>
        /// Gets or sets the mileage rate per mile.
        /// </summary>
        public decimal MileageRate { get; set; }

        /// <summary>
        /// Gets or sets the total calculated amount.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the fuel surcharge amount.
        /// </summary>
        public decimal FuelSurcharge { get; set; }

        /// <summary>
        /// Gets or sets the toll charges.
        /// </summary>
        public decimal TollCharges { get; set; }

        /// <summary>
        /// Gets or sets any additional charges.
        /// </summary>
        public decimal AdditionalCharges { get; set; }
    }

    /// <summary>
    /// Represents a document associated with a job.
    /// </summary>
    public class JobDocument
    {
        /// <summary>
        /// Gets or sets the unique identifier for the document.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of document.
        /// </summary>
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Gets or sets the file name of the document.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the storage URL of the document.
        /// </summary>
        public string StorageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the document was uploaded.
        /// </summary>
        public DateTime UploadedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who uploaded the document.
        /// </summary>
        public string UploadedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents tracking information for a job.
    /// </summary>
    public class TrackingInfo
    {
        /// <summary>
        /// Gets or sets the list of location updates.
        /// </summary>
        public List<LocationUpdate> Updates { get; set; } = new List<LocationUpdate>();

        /// <summary>
        /// Gets or sets the current status of tracking.
        /// </summary>
        public string CurrentStatus { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp of the last update.
        /// </summary>
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// Gets or sets the estimated distance remaining.
        /// </summary>
        public double EstimatedDistance { get; set; }

        /// <summary>
        /// Gets or sets the estimated time remaining.
        /// </summary>
        public TimeSpan EstimatedTimeRemaining { get; set; }
    }

    /// <summary>
    /// Represents a location update for tracking.
    /// </summary>
    public class LocationUpdate
    {
        /// <summary>
        /// Gets or sets the latitude coordinate.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude coordinate.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the speed in miles per hour.
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// Gets or sets the heading in degrees.
        /// </summary>
        public int Heading { get; set; }

        /// <summary>
        /// Gets or sets the accuracy of the location in meters.
        /// </summary>
        public double Accuracy { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the location update.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Represents the type of document.
    /// </summary>
    public enum DocumentType
    {
        /// <summary>
        /// Bill of lading document.
        /// </summary>
        BillOfLading = 0,

        /// <summary>
        /// Proof of delivery document.
        /// </summary>
        ProofOfDelivery = 1,

        /// <summary>
        /// Insurance certificate.
        /// </summary>
        InsuranceCertificate = 2,

        /// <summary>
        /// Commercial invoice.
        /// </summary>
        CommercialInvoice = 3,

        /// <summary>
        /// Packing list.
        /// </summary>
        PackingList = 4,

        /// <summary>
        /// Other document type.
        /// </summary>
        Other = 5
    }
}
