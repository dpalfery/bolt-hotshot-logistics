namespace HotshotLogistics.Contracts.Services
{
    /// <summary>
    ///     Service interface for managing SignalR connections and user-to-connection mappings
    /// </summary>
    public interface IConnectionManagerService
    {
        /// <summary>
        ///     Add a connection for a user
        /// </summary>
        Task AddConnectionAsync(string userId, string connectionId);

        /// <summary>
        ///     Remove a connection for a user
        /// </summary>
        Task RemoveConnectionAsync(string connectionId);

        /// <summary>
        ///     Get all connections for a user
        /// </summary>
        Task<IEnumerable<string>> GetConnectionsAsync(string userId);

        /// <summary>
        ///     Get user ID for a connection
        /// </summary>
        Task<string?> GetUserIdAsync(string connectionId);

        /// <summary>
        ///     Add connection to a group
        /// </summary>
        Task AddToGroupAsync(string connectionId, string groupName);

        /// <summary>
        ///     Remove connection from a group
        /// </summary>
        Task RemoveFromGroupAsync(string connectionId, string groupName);

        /// <summary>
        ///     Get all connections in a group
        /// </summary>
        Task<IEnumerable<string>> GetGroupConnectionsAsync(string groupName);

        /// <summary>
        ///     Check if connection exists
        /// </summary>
        Task<bool> ConnectionExistsAsync(string connectionId);

        /// <summary>
        ///     Get connection count for a user
        /// </summary>
        Task<int> GetUserConnectionCountAsync(string userId);

        /// <summary>
        ///     Clean up expired connections
        /// </summary>
        Task CleanupExpiredConnectionsAsync();

        /// <summary>
        ///     Get all connections for a user (alias for GetConnectionsAsync)
        /// </summary>
        Task<List<string>> GetUserConnectionsAsync(string userId);

        /// <summary>
        ///     Check if a user is currently connected
        /// </summary>
        Task<bool> IsUserConnectedAsync(string userId);

        /// <summary>
        ///     Send a message to a specific user
        /// </summary>
        Task SendToUserAsync(string userId, string methodName, params object[] args);

        /// <summary>
        ///     Send a message to multiple users
        /// </summary>
        Task SendToUsersAsync(IEnumerable<string> userIds, string methodName, params object[] args);

        /// <summary>
        ///     Remove all connections for a user
        /// </summary>
        Task RemoveUserConnectionsAsync(string userId);

        /// <summary>
        ///     Handle reconnection with exponential backoff
        /// </summary>
        Task<bool> HandleReconnectionAsync(string userId, string connectionId, int retryCount = 0);
    }
}
