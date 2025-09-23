using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using HotshotLogistics.Contracts.Services;
using System.Text.Json;

namespace HotshotLogistics.Application.Services;

/// <summary>
/// Service for managing SignalR connections and user-to-connection mappings using Redis
/// </summary>
public class ConnectionManagerService : IConnectionManagerService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<ConnectionManagerService> _logger;
   
 public ConnectionManagerService(IDistributedCache cache, ILogger<ConnectionManagerService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task AddConnectionAsync(string userId, string connectionId)
    {
        var key = $"user_connections:{userId}";
        var connections = await GetUserConnectionsAsync(userId);
        
        if (!connections.Contains(connectionId))
        {
            connections.Add(connectionId);
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(connections));
            
            // Also store connection-to-user mapping
            var connectionKey = $"connection_user:{connectionId}";
            await _cache.SetStringAsync(connectionKey, userId);
            
            _logger.LogInformation("Added connection {ConnectionId} for user {UserId}", connectionId, userId);
        }
    }

    public async Task RemoveConnectionAsync(string userId, string connectionId)
    {
        var key = $"user_connections:{userId}";
        var connections = await GetUserConnectionsAsync(userId);
        
        if (connections.Remove(connectionId))
        {
            if (connections.Count > 0)
            {
                await _cache.SetStringAsync(key, JsonSerializer.Serialize(connections));
            }
            else
            {
                await _cache.RemoveAsync(key);
            }
            _logger.LogInformation("Removed connection {ConnectionId} for user {UserId}", connectionId, userId);
        }
    }

    public async Task<List<string>> GetUserConnectionsAsync(string userId)
    {
        var key = $"user_connections:{userId}";
        var connectionsJson = await _cache.GetStringAsync(key);
        
        if (string.IsNullOrEmpty(connectionsJson))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(connectionsJson) ?? new List<string>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize connections for user {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<bool> IsUserConnectedAsync(string userId)
    {
        var connections = await GetUserConnectionsAsync(userId);
        return connections.Count > 0;
    }

    public async Task AddUserToGroupAsync(string userId, string groupName)
    {
        var key = $"user_groups:{userId}";
        var groups = await GetUserGroupsAsync(userId);
        
        if (!groups.Contains(groupName))
        {
            groups.Add(groupName);
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(groups));
            _logger.LogInformation("Added user {UserId} to group {GroupName}", userId, groupName);
        }
    }

    public async Task RemoveUserFromGroupAsync(string userId, string groupName)
    {
        var key = $"user_groups:{userId}";
        var groups = await GetUserGroupsAsync(userId);
        
        if (groups.Remove(groupName))
        {
            if (groups.Count > 0)
            {
                await _cache.SetStringAsync(key, JsonSerializer.Serialize(groups));
            }
            else
            {
                await _cache.RemoveAsync(key);
            }
            _logger.LogInformation("Removed user {UserId} from group {GroupName}", userId, groupName);
        }
    }

    public async Task<List<string>> GetUserGroupsAsync(string userId)
    {
        var key = $"user_groups:{userId}";
        var groupsJson = await _cache.GetStringAsync(key);
        
        if (string.IsNullOrEmpty(groupsJson))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(groupsJson) ?? new List<string>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize groups for user {UserId}", userId);
            return new List<string>();
        }
    }

    // Interface-required methods
    public async Task RemoveConnectionAsync(string connectionId)
    {
        // Find user by connection ID and remove the connection
        var userId = await GetUserIdAsync(connectionId);
        if (!string.IsNullOrEmpty(userId))
        {
            await RemoveConnectionAsync(userId, connectionId);
        }
        
        // Also remove from connection-to-user mapping
        var connectionKey = $"connection_user:{connectionId}";
        await _cache.RemoveAsync(connectionKey);
    }

    public async Task<IEnumerable<string>> GetConnectionsAsync(string userId)
    {
        return await GetUserConnectionsAsync(userId);
    }

    public async Task<string?> GetUserIdAsync(string connectionId)
    {
        var key = $"connection_user:{connectionId}";
        return await _cache.GetStringAsync(key);
    }

    public async Task AddToGroupAsync(string connectionId, string groupName)
    {
        var key = $"group_connections:{groupName}";
        var connections = await GetGroupConnectionsListAsync(groupName);
        
        if (!connections.Contains(connectionId))
        {
            connections.Add(connectionId);
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(connections));
            _logger.LogInformation("Added connection {ConnectionId} to group {GroupName}", connectionId, groupName);
        }
    }

    public async Task RemoveFromGroupAsync(string connectionId, string groupName)
    {
        var key = $"group_connections:{groupName}";
        var connections = await GetGroupConnectionsListAsync(groupName);
        
        if (connections.Remove(connectionId))
        {
            if (connections.Count > 0)
            {
                await _cache.SetStringAsync(key, JsonSerializer.Serialize(connections));
            }
            else
            {
                await _cache.RemoveAsync(key);
            }
            _logger.LogInformation("Removed connection {ConnectionId} from group {GroupName}", connectionId, groupName);
        }
    }

    public async Task<IEnumerable<string>> GetGroupConnectionsAsync(string groupName)
    {
        return await GetGroupConnectionsListAsync(groupName);
    }

    public async Task<bool> ConnectionExistsAsync(string connectionId)
    {
        var userId = await GetUserIdAsync(connectionId);
        return !string.IsNullOrEmpty(userId);
    }

    public async Task<int> GetUserConnectionCountAsync(string userId)
    {
        var connections = await GetUserConnectionsAsync(userId);
        return connections.Count;
    }

    public async Task CleanupExpiredConnectionsAsync()
    {
        // This would require additional logic to track connection timestamps
        // For now, just log that cleanup was requested
        _logger.LogInformation("Connection cleanup requested - implementation needed for production");
        await Task.CompletedTask;
    }

    // Helper method
    private async Task<List<string>> GetGroupConnectionsListAsync(string groupName)
    {
        var key = $"group_connections:{groupName}";
        var connectionsJson = await _cache.GetStringAsync(key);
        
        if (string.IsNullOrEmpty(connectionsJson))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(connectionsJson) ?? new List<string>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize connections for group {GroupName}", groupName);
            return new List<string>();
        }
    }
}