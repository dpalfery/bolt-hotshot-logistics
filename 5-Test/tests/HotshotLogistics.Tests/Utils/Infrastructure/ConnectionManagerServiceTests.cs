using System.Text;
using System.Text.Json;
using FluentAssertions;
using HotshotLogistics.Application.Services;
using HotshotLogistics.Contracts.Hubs;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;

namespace HotshotLogistics.Tests.Utils.Infrastructure
{
    /// <summary>
    ///     Integration tests for ConnectionManagerService
    /// </summary>
    public class ConnectionManagerServiceTests
    {
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly IConnectionManagerService _service;
        private readonly Mock<ISignalRClientWrapper> _signalRClientMock;

        public ConnectionManagerServiceTests()
        {
            _cacheMock = new Mock<IDistributedCache>();
            Mock<ILogger<ConnectionManagerService>> loggerMock = new();
            _signalRClientMock = new Mock<ISignalRClientWrapper>();

            _service = new ConnectionManagerService(
                _cacheMock.Object,
                loggerMock.Object,
                _signalRClientMock.Object);
        }

        [Fact]
        public async Task AddConnectionAsync_ShouldStoreConnection_WhenConnectionDoesNotExist()
        {
            // Arrange
            string userId = "user1";
            string connectionId = "conn1";
            string expectedKey = $"user_connections:{userId}";
            string expectedConnectionKey = $"connection_user:{connectionId}";

            _cacheMock.Setup(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            // Act
            await _service.AddConnectionAsync(userId, connectionId);

            // Assert
            _cacheMock.Verify(c => c.SetAsync(expectedKey, It.Is<byte[]>(b =>
                    Encoding.UTF8.GetString(b).Contains(connectionId)), It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _cacheMock.Verify(c => c.SetAsync(expectedConnectionKey, It.Is<byte[]>(b =>
                    Encoding.UTF8.GetString(b) == userId), It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddConnectionAsync_ShouldNotAddDuplicateConnection()
        {
            // Arrange
            string userId = "user1";
            string connectionId = "conn1";
            List<string> existingConnections = [connectionId];
            string expectedKey = $"user_connections:{userId}";

            _cacheMock.Setup(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(existingConnections)));

            // Act
            await _service.AddConnectionAsync(userId, connectionId);

            // Assert
            _cacheMock.Verify(
                c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RemoveConnectionAsync_ShouldRemoveConnection_WhenExists()
        {
            // Arrange
            string userId = "user1";
            string connectionId = "conn1";
            List<string> existingConnections = [connectionId, "conn2"];
            string expectedKey = $"user_connections:{userId}";
            string expectedConnectionKey = $"connection_user:{connectionId}";

            _cacheMock.Setup(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(existingConnections)));
            _cacheMock.Setup(c => c.GetAsync(expectedConnectionKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(userId));

            // Act
            await _service.RemoveConnectionAsync(connectionId);

            // Assert
            _cacheMock.Verify(c => c.SetAsync(expectedKey, It.Is<byte[]>(b =>
                    !Encoding.UTF8.GetString(b).Contains(connectionId)), It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync(expectedConnectionKey, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetUserConnectionsAsync_ShouldReturnConnections_WhenExist()
        {
            // Arrange
            string userId = "user1";
            List<string> connections = ["conn1", "conn2"];
            string expectedKey = $"user_connections:{userId}";

            _cacheMock.Setup(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(connections)));

            // Act
            List<string> result = await _service.GetUserConnectionsAsync(userId);

            // Assert
            result.Should().BeEquivalentTo(connections);
        }

        [Fact]
        public async Task GetUserConnectionsAsync_ShouldReturnEmptyList_WhenNoConnections()
        {
            // Arrange
            string userId = "user1";
            string expectedKey = $"user_connections:{userId}";

            _cacheMock.Setup(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            // Act
            List<string> result = await _service.GetUserConnectionsAsync(userId);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task IsUserConnectedAsync_ShouldReturnTrue_WhenConnectionsExist()
        {
            // Arrange
            string userId = "user1";
            List<string> connections = ["conn1"];
            string expectedKey = $"user_connections:{userId}";

            _cacheMock.Setup(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(connections)));

            // Act
            bool result = await _service.IsUserConnectedAsync(userId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task SendToUserAsync_ShouldCallSignalRClient()
        {
            // Arrange
            string userId = "user1";
            string methodName = "TestMethod";
            object[] args = ["arg1", 123];

            // Act
            await _service.SendToUserAsync(userId, methodName, args);

            // Assert
            _signalRClientMock.Verify(c => c.SendToUserAsync(userId, methodName, args), Times.Once);
        }

        [Fact]
        public async Task SendToUsersAsync_ShouldCallSignalRClientForEachUser()
        {
            // Arrange
            List<string> userIds = ["user1", "user2"];
            string methodName = "TestMethod";
            object[] args = ["arg1"];

            // Act
            await _service.SendToUsersAsync(userIds, methodName, args);

            // Assert
            _signalRClientMock.Verify(c => c.SendToUserAsync("user1", methodName, args), Times.Once);
            _signalRClientMock.Verify(c => c.SendToUserAsync("user2", methodName, args), Times.Once);
        }

        [Fact]
        public async Task RemoveUserConnectionsAsync_ShouldRemoveAllUserConnections()
        {
            // Arrange
            string userId = "user1";
            List<string> connections = ["conn1", "conn2"];
            string expectedKey = $"user_connections:{userId}";

            _cacheMock.SetupSequence(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    Encoding.UTF8.GetBytes(
                        JsonSerializer.Serialize(connections))) // initial call in RemoveUserConnectionsAsync
                .ReturnsAsync(
                    Encoding.UTF8.GetBytes(
                        JsonSerializer.Serialize(connections))) // first call inside RemoveConnectionAsync (conn1)
                .ReturnsAsync(
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new List<string>
                        { "conn2" }))); // second call inside RemoveConnectionAsync (conn2)

            // Act
            await _service.RemoveUserConnectionsAsync(userId);

            // Assert
            _cacheMock.Verify(c => c.RemoveAsync(expectedKey, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleReconnectionAsync_ShouldReturnTrue_WhenConnectionStillActive()
        {
            // Arrange
            string userId = "user1";
            string connectionId = "conn1";
            List<string> connections = [connectionId];
            string expectedKey = $"user_connections:{userId}";

            _cacheMock.Setup(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(connections)));

            // Act
            bool result = await _service.HandleReconnectionAsync(userId, connectionId);

            // Assert
            result.Should().BeTrue();
            _cacheMock.Verify(
                c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleReconnectionAsync_ShouldReconnect_WhenConnectionNotActive()
        {
            // Arrange
            string userId = "user1";
            string connectionId = "conn1";
            string expectedKey = $"user_connections:{userId}";

            _cacheMock.Setup(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            // Act
            bool result = await _service.HandleReconnectionAsync(userId, connectionId);

            // Assert
            result.Should().BeTrue();
            _cacheMock.Verify(c => c.SetAsync(expectedKey, It.Is<byte[]>(b =>
                    Encoding.UTF8.GetString(b).Contains(connectionId)), It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleReconnectionAsync_ShouldReturnFalse_AfterMaxRetries()
        {
            // Arrange
            string userId = "user1";
            string connectionId = "conn1";
            string expectedKey = $"user_connections:{userId}";

            _cacheMock.Setup(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            _cacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Redis connection failed"));

            // Act
            bool result = await _service.HandleReconnectionAsync(userId, connectionId, 5);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetUserIdAsync_ShouldReturnUserId_WhenConnectionExists()
        {
            // Arrange
            string connectionId = "conn1";
            string userId = "user1";
            string expectedKey = $"connection_user:{connectionId}";

            _cacheMock.Setup(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(userId));

            // Act
            string? result = await _service.GetUserIdAsync(connectionId);

            // Assert
            result.Should().Be(userId);
        }

        [Fact]
        public async Task ConnectionExistsAsync_ShouldReturnTrue_WhenConnectionExists()
        {
            // Arrange
            string connectionId = "conn1";
            string userId = "user1";
            string expectedKey = $"connection_user:{connectionId}";

            _cacheMock.Setup(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(userId));

            // Act
            bool result = await _service.ConnectionExistsAsync(connectionId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetUserConnectionCountAsync_ShouldReturnCorrectCount()
        {
            // Arrange
            string userId = "user1";
            List<string> connections = ["conn1", "conn2", "conn3"];
            string expectedKey = $"user_connections:{userId}";

            _cacheMock.Setup(c => c.GetAsync(expectedKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(connections)));

            // Act
            int result = await _service.GetUserConnectionCountAsync(userId);

            // Assert
            result.Should().Be(3);
        }
    }
}
