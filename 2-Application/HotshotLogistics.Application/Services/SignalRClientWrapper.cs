using Microsoft.Azure.SignalR.Management;
using Microsoft.AspNetCore.SignalR;
using HotshotLogistics.Contracts.Hubs;

namespace HotshotLogistics.Application.Services;

/// <summary>
/// Implementation of SignalR client wrapper for real SignalR operations
/// </summary>
public class SignalRClientWrapper : ISignalRClientWrapper
{
    private readonly ServiceHubContext _hubContext;

    public SignalRClientWrapper(ServiceHubContext hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Send a message to a specific group
    /// </summary>
    public async Task SendToGroupAsync(string groupName, string methodName, params object[] args)
    {
        await _hubContext.Clients.Group(groupName).SendAsync(methodName, args);
    }

    /// <summary>
    /// Send a message to a specific user
    /// </summary>
    public async Task SendToUserAsync(string userId, string methodName, params object[] args)
    {
        await _hubContext.Clients.User(userId).SendAsync(methodName, args);
    }

    /// <summary>
    /// Send a message to all connected clients
    /// </summary>
    public async Task SendToAllAsync(string methodName, params object[] args)
    {
        await _hubContext.Clients.All.SendAsync(methodName, args);
    }

    /// <summary>
    /// Send a message to the caller
    /// </summary>
    public Task SendToCallerAsync(string methodName, params object[] args)
    {
        // Note: Caller is not available in Azure SignalR Service context
        // This would need to be implemented differently for server-side hubs
        throw new NotSupportedException("Caller is not supported in Azure SignalR Service");
    }
}
