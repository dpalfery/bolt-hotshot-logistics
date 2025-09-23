namespace HotshotLogistics.Contracts.Services;

/// <summary>
/// Service interface for managing SignalR connections and user-to-connection mappings
/// </summary>
public interface IConnectionManagerService
{
    /// <summary>
    /// Add a connection for a user
    /// </summary>
    Task AddConnectionAsync(string userId, string connectionId);

    /// <summary>
    /// Remove a connection for a user
    /// </summary>
    Task RemoveConnectionAsync(string connectionId);

    /// <summary>
    /// Get all connections for a user
    /// </summary>
    Task<IEnumerable<string>> GetConnectionsAsync(string userId);

    /// <summary>
    /// Get user ID for a connection
    /// </summary>
    Task<string?> GetUserIdAsync(string connectionId);

    /// <summary>
    /// Add connection to a group
    /// </summary>
    Task AddToGroupAsync(string connectionId, string groupName);

    /// <summary>
    /// Remove connection from a group
    /// </summary>
    Task RemoveFromGroupAsync(string connectionId, string groupName);

    /// <summary>
    /// Get all connections in a group
    /// </summary>
    Task<IEnumerable<string>> GetGroupConnectionsAsync(string groupName);

    /// <summary>
    /// Check if connection exists
    /// </summary>
    Task<bool> ConnectionExistsAsync(string connectionId);

    /// <summary>
    /// Get connection count for a user
    /// </summary>
    Task<int> GetUserConnectionCountAsync(string userId);

    /// <summary>
    /// Clean up expired connections
    /// </summary>
    Task CleanupExpiredConnectionsAsync();
}