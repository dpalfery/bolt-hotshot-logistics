// <copyright file="MappingServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Tests;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HotshotLogistics.Contracts.Models;
using HotshotLogistics.Data.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

public class MappingServiceTests
{
    [Fact]
    public async Task AzureMapsService_GeocodeAddressAsync_ValidAddress_ReturnsGeocodingResult()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"
            {
                ""results"": [
                    {
                        ""position"": { ""lat"": 40.7128, ""lon"": -74.0060 },
                        ""address"": { ""freeformAddress"": ""New York, NY, USA"" },
                        ""confidence"": 0.9
                    }
                ]
            }")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var loggerMock = new Mock<ILogger<AzureMapsService>>();
        var service = new AzureMapsService(httpClient, loggerMock.Object, "test-key");

        // Act
        var result = await service.GeocodeAddressAsync("New York, NY", CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(40.7128m, result.Latitude);
        Assert.Equal(-74.0060m, result.Longitude);
        Assert.Equal("New York, NY, USA", result.FormattedAddress);
        Assert.Equal(0.9, result.Confidence);
    }

    [Fact]
    public async Task AzureMapsService_GeocodeAddressAsync_InvalidAddress_ReturnsInvalidResult()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{ ""results"": [] }")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var loggerMock = new Mock<ILogger<AzureMapsService>>();
        var service = new AzureMapsService(httpClient, loggerMock.Object, "test-key");

        // Act
        var result = await service.GeocodeAddressAsync("Invalid Address", CancellationToken.None);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("No geocoding results found", result.ErrorMessage);
    }

    [Fact]
    public async Task AzureMapsService_ReverseGeocodeAsync_ValidCoordinates_ReturnsReverseGeocodingResult()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"
            {
                ""addresses"": [
                    {
                        ""address"": {
                            ""streetName"": ""Broadway"",
                            ""municipality"": ""New York"",
                            ""countrySubdivision"": ""NY"",
                            ""postalCode"": ""10001"",
                            ""countryCode"": ""US"",
                            ""freeformAddress"": ""Broadway, New York, NY 10001, USA""
                        }
                    }
                ]
            }")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var loggerMock = new Mock<ILogger<AzureMapsService>>();
        var service = new AzureMapsService(httpClient, loggerMock.Object, "test-key");

        // Act
        var result = await service.ReverseGeocodeAsync(40.7128m, -74.0060m, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal("Broadway", result.Address);
        Assert.Equal("New York", result.City);
        Assert.Equal("NY", result.State);
        Assert.Equal("10001", result.PostalCode);
        Assert.Equal("US", result.Country);
    }

    [Fact]
    public async Task AzureMapsService_CalculateRouteAsync_ValidLocations_ReturnsRouteResult()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"
            {
                ""routes"": [
                    {
                        ""summary"": {
                            ""lengthInMeters"": 10000,
                            ""travelTimeInSeconds"": 900
                        }
                    }
                ]
            }")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var loggerMock = new Mock<ILogger<AzureMapsService>>();
        var service = new AzureMapsService(httpClient, loggerMock.Object, "test-key");

        var origin = new Location { Latitude = 40.7128m, Longitude = -74.0060m };
        var destination = new Location { Latitude = 40.7589m, Longitude = -73.9851m };

        // Act
        var result = await service.CalculateRouteAsync(origin, destination, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(6.21371, result.Distance, 2); // 10000 meters ≈ 6.21 miles
        Assert.Equal(TimeSpan.FromSeconds(900), result.Duration);
        Assert.Equal(DateTime.UtcNow.AddSeconds(900).Date, result.EstimatedArrival.Date); // Approximate time check
    }

    [Fact]
    public async Task GoogleMapsService_GeocodeAddressAsync_ValidAddress_ReturnsGeocodingResult()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"
            {
                ""status"": ""OK"",
                ""results"": [
                    {
                        ""formatted_address"": ""New York, NY, USA"",
                        ""geometry"": {
                            ""location"": { ""lat"": 40.7128, ""lng"": -74.0060 }
                        }
                    }
                ]
            }")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var loggerMock = new Mock<ILogger<GoogleMapsService>>();
        var service = new GoogleMapsService(httpClient, loggerMock.Object, "test-key");

        // Act
        var result = await service.GeocodeAddressAsync("New York, NY", CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(40.7128m, result.Latitude);
        Assert.Equal(-74.0060m, result.Longitude);
        Assert.Equal("New York, NY, USA", result.FormattedAddress);
        Assert.Equal(1.0, result.Confidence);
    }

    [Fact]
    public async Task GoogleMapsService_CalculateRouteAsync_ValidLocations_ReturnsRouteResult()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"
            {
                ""status"": ""OK"",
                ""routes"": [
                    {
                        ""legs"": [
                            {
                                ""distance"": { ""value"": 10000 },
                                ""duration"": { ""value"": 900 }
                            }
                        ],
                        ""overview_polyline"": { ""points"": ""test_polyline"" }
                    }
                ]
            }")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var loggerMock = new Mock<ILogger<GoogleMapsService>>();
        var service = new GoogleMapsService(httpClient, loggerMock.Object, "test-key");

        var origin = new Location { Latitude = 40.7128m, Longitude = -74.0060m };
        var destination = new Location { Latitude = 40.7589m, Longitude = -73.9851m };

        // Act
        var result = await service.CalculateRouteAsync(origin, destination, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(6.21371, result.Distance, 2); // 10000 meters ≈ 6.21 miles
        Assert.Equal(TimeSpan.FromSeconds(900), result.Duration);
        Assert.Equal("test_polyline", result.Polyline);
    }

    [Fact]
    public async Task MappingService_OptimizeRouteAsync_ValidWaypoints_ReturnsOptimizedResult()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"
            {
                ""routes"": [
                    {
                        ""summary"": {
                            ""lengthInMeters"": 10000,
                            ""travelTimeInSeconds"": 900
                        }
                    }
                ]
            }")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var loggerMock = new Mock<ILogger<AzureMapsService>>();
        var service = new AzureMapsService(httpClient, loggerMock.Object, "test-key");

        var waypoints = new List<Location>
        {
            new Location { Latitude = 40.7128m, Longitude = -74.0060m },
            new Location { Latitude = 40.7589m, Longitude = -73.9851m }
        };

        // Act
        var result = await service.OptimizeRouteAsync(waypoints, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(2, result.OptimizedWaypoints.Count);
        Assert.Equal(6.21371, result.TotalDistance, 2);
        Assert.Equal(TimeSpan.FromSeconds(900), result.TotalDuration);
        Assert.Single(result.RouteSegments);
    }

    [Fact]
    public async Task MappingService_CalculateDistanceAsync_ValidLocations_ReturnsDistanceResult()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"
            {
                ""routes"": [
                    {
                        ""summary"": {
                            ""lengthInMeters"": 10000,
                            ""travelTimeInSeconds"": 900
                        }
                    }
                ]
            }")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var loggerMock = new Mock<ILogger<AzureMapsService>>();
        var service = new AzureMapsService(httpClient, loggerMock.Object, "test-key");

        var origin = new Location { Latitude = 40.7128m, Longitude = -74.0060m };
        var destination = new Location { Latitude = 40.7589m, Longitude = -73.9851m };

        // Act
        var result = await service.CalculateDistanceAsync(origin, destination, CancellationToken.None);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(6.21371, result.Distance, 2);
        Assert.Equal(TimeSpan.FromSeconds(900), result.Duration);
    }

    [Fact]
    public async Task MappingService_ValidateAddressAsync_ValidAddress_ReturnsTrue()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"
            {
                ""status"": ""OK"",
                ""results"": [
                    {
                        ""formatted_address"": ""New York, NY, USA"",
                        ""geometry"": {
                            ""location"": { ""lat"": 40.7128, ""lng"": -74.0060 }
                        }
                    }
                ]
            }")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var loggerMock = new Mock<ILogger<GoogleMapsService>>();
        var service = new GoogleMapsService(httpClient, loggerMock.Object, "test-key");

        // Act
        var result = await service.ValidateAddressAsync("New York, NY", CancellationToken.None);

        // Assert
        Assert.True(result);
    }
}