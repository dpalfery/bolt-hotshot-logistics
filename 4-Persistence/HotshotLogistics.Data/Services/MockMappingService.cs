// <copyright file="MockMappingService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using HotshotLogistics.Contracts.Services;
using HotshotLogistics.Domain.DTOs;
using HotshotLogistics.Domain.Entities;
using HotshotLogistics.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HotshotLogistics.Data.Services
{
    /// <summary>
    ///     Mock implementation of the mapping service for development and testing.
    ///     This service provides simulated geocoding and routing without requiring real API keys.
    /// </summary>
    public class MockMappingService : IMappingService
    {
        private readonly ILogger<MockMappingService> _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MockMappingService" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public MockMappingService(ILogger<MockMappingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogInformation("Using MockMappingService for development. No real API calls will be made.");
        }

        /// <inheritdoc />
        public Task<GeocodingResult> GeocodeAddressAsync(string address, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Mock geocoding address: {Address}", address);

            // Return mock coordinates based on a hash of the address
            int hash = Math.Abs(address.GetHashCode());
            decimal lat = 30.0m + (hash % 20);
            decimal lng = -100.0m + (hash % 30);

            return Task.FromResult(new GeocodingResult
            {
                Latitude = lat,
                Longitude = lng,
                FormattedAddress = address,
                IsValid = true,
                Confidence = 0.95
            });
        }

        /// <inheritdoc />
        public Task<ReverseGeocodingResult> ReverseGeocodeAsync(decimal latitude, decimal longitude,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Mock reverse geocoding: {Latitude}, {Longitude}", latitude, longitude);

            return Task.FromResult(new ReverseGeocodingResult
            {
                Address = "123 Mock Street",
                City = "Mock City",
                State = "TX",
                PostalCode = "12345",
                Country = "US",
                FormattedAddress = "123 Mock Street, Mock City, TX 12345, US",
                IsValid = true
            });
        }

        /// <inheritdoc />
        public Task<bool> ValidateAddressAsync(string address, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Mock validating address: {Address}", address);
            // Mock implementation always returns true for valid addresses
            return Task.FromResult(!string.IsNullOrWhiteSpace(address));
        }

        /// <inheritdoc />
        public Task<RouteResult> CalculateRouteAsync(Location origin, Location destination,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Mock calculating route from {Origin} to {Destination}", origin.FullAddress,
                destination.FullAddress);

            if (!origin.HasCoordinates || !destination.HasCoordinates)
            {
                return Task.FromResult(new RouteResult
                {
                    IsValid = false,
                    ErrorMessage = "Both origin and destination must have coordinates"
                });
            }

            // Calculate approximate distance using straight-line distance
            double distance = CalculateStraightLineDistance(origin, destination);
            TimeSpan duration = TimeSpan.FromHours(distance / 60.0); // Assume average speed of 60 mph

            return Task.FromResult(new RouteResult
            {
                Distance = distance,
                Duration = duration,
                Waypoints = [origin, destination],
                Polyline = "mock_polyline_encoded_string",
                EstimatedArrival = DateTime.UtcNow.Add(duration),
                IsValid = true
            });
        }

        /// <inheritdoc />
        public async Task<OptimizedRouteResult> OptimizeRouteAsync(IList<Location> waypoints,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Mock optimizing route with {WaypointCount} waypoints", waypoints.Count);

            if (waypoints.Count < 2)
            {
                return new OptimizedRouteResult
                {
                    IsValid = false,
                    ErrorMessage = "At least 2 waypoints required"
                };
            }

            double totalDistance = 0.0;
            TimeSpan totalDuration = TimeSpan.Zero;
            List<RouteResult> segments = new();

            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                RouteResult segment = await CalculateRouteAsync(waypoints[i], waypoints[i + 1], cancellationToken);
                if (!segment.IsValid)
                {
                    return new OptimizedRouteResult
                    {
                        IsValid = false,
                        ErrorMessage = $"Failed to calculate route segment {i}"
                    };
                }

                totalDistance += segment.Distance;
                totalDuration = totalDuration.Add(segment.Duration);
                segments.Add(segment);
            }

            return new OptimizedRouteResult
            {
                OptimizedWaypoints = waypoints,
                TotalDistance = totalDistance,
                TotalDuration = totalDuration,
                RouteSegments = segments,
                EstimatedArrival = DateTime.UtcNow.Add(totalDuration),
                IsValid = true
            };
        }

        /// <inheritdoc />
        public async Task<DistanceResult> CalculateDistanceAsync(Location origin, Location destination,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Mock calculating distance from {Origin} to {Destination}", origin.FullAddress,
                destination.FullAddress);

            RouteResult route = await CalculateRouteAsync(origin, destination, cancellationToken);
            return new DistanceResult
            {
                Distance = route.Distance,
                Duration = route.Duration,
                IsValid = route.IsValid,
                ErrorMessage = route.ErrorMessage
            };
        }

        /// <summary>
        ///     Calculates the straight-line distance between two locations using the Haversine formula.
        /// </summary>
        /// <param name="origin">The starting location.</param>
        /// <param name="destination">The destination location.</param>
        /// <returns>The distance in miles.</returns>
        private double CalculateStraightLineDistance(Location origin, Location destination)
        {
            const double earthRadiusMiles = 3958.8;

            double lat1 = (double)origin.Latitude!;
            double lon1 = (double)origin.Longitude!;
            double lat2 = (double)destination.Latitude!;
            double lon2 = (double)destination.Longitude!;

            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = (Math.Sin(dLat / 2) * Math.Sin(dLat / 2)) +
                       (Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2));

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadiusMiles * c;
        }

        /// <summary>
        ///     Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">The angle in degrees.</param>
        /// <returns>The angle in radians.</returns>
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
