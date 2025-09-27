// <copyright file="ReverseGeocodingResult.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Contracts.Models
{
    /// <summary>
    /// Represents the result of a reverse geocoding operation.
    /// </summary>
    public class ReverseGeocodingResult
    {
        /// <summary>
        /// Gets or sets the street address.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the state or province.
        /// </summary>
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the postal code.
        /// </summary>
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the formatted address returned by the reverse geocoding service.
        /// </summary>
        public string FormattedAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the reverse geocoding was successful.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets any error message if the reverse geocoding failed.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
