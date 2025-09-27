using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Logging;
using HotshotLogistics.Contracts.Hubs;
using HotshotLogistics.Contracts.Models;
using HotshotLogistics.Contracts.Services;
using System.Text.Json;

namespace HotshotLogistics.Api.Hubs;

/// <summary>
/// SignalR hub for real-time communication with clients
/// </summary>
public class RealtimeHub
{
    private readonly ILogger<RealtimeHub> _logger;
    private readonly ISignalRClientWrapper _signalRClient;
    private readonly ServiceHubContext _hubContext;
    private readonly IConnectionManagerService _connectionManager;

    public RealtimeHub(ILogger<RealtimeHub> logger, ISignalRClientWrapper signalRClient, ServiceHubContext hubContext, IConnectionManagerService connectionManager)
    {
        _logger = logger;
        _signalRClient = signalRClient;
        _hubContext = hubContext;
        _connectionManager = connectionManager;
    }

    /// <summary>
    /// Negotiation endpoint for SignalR connections
    /// </summary>
    [Function("negotiate")]
    public async Task<HttpResponseData> Negotiate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        try
        {
            var userId = GetUserIdFromRequest(req);
            var negotiateResponse = await _hubContext.NegotiateAsync(new()
            {
                UserId = userId
            });

            var response = req.CreateResponse();
            await response.WriteStringAsync(JsonSerializer.Serialize(negotiateResponse));
            response.Headers.Add("Content-Type", "application/json");
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SignalR negotiation");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Negotiation failed");
            return errorResponse;
        }
    }

    /// <summary>
    /// Broadcast job status update to all connected clients tracking the job
    /// </summary>
    public async Task JobStatusUpdated(string jobId, JobStatus status)
    {
        try
        {
            await _signalRClient.SendToGroupAsync($"job-{jobId}", "JobStatusUpdated", jobId, status);

            _logger.LogInformation("Job status update sent for job {JobId}: {Status}", jobId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting job status update for job {JobId}", jobId);
        }
    }

    /// <summary>
    /// Broadcast location update to clients tracking the job
    /// </summary>
    public async Task LocationUpdated(string jobId, LocationUpdate location)
    {
        try
        {
            await _signalRClient.SendToGroupAsync($"job-{jobId}", "LocationUpdated", jobId, location);

            _logger.LogDebug("Location update sent for job {JobId}", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting location update for job {JobId}", jobId);
        }
    }

    /// <summary>
    /// Broadcast driver status change to relevant clients
    /// </summary>
    public async Task DriverStatusChanged(int driverId, DriverStatus status)
    {
        try
        {
            await _signalRClient.SendToGroupAsync($"driver-{driverId}", "DriverStatusChanged", driverId, status);

            // Also notify admin clients
            await _signalRClient.SendToGroupAsync("admins", "DriverStatusChanged", driverId, status);

            _logger.LogInformation("Driver status update sent for driver {DriverId}: {Status}", driverId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting driver status update for driver {DriverId}", driverId);
        }
    }

    /// <summary>
    /// Broadcast new job availability to available drivers
    /// </summary>
    public async Task NewJobAvailable(JobDto job)
    {
        try
        {
            await _signalRClient.SendToGroupAsync("available-drivers", "NewJobAvailable", job);

            _logger.LogInformation("New job availability broadcast for job {JobId}", job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting new job availability for job {JobId}", job.Id);
        }
    }

    /// <summary>
    /// Send notification to specific user or broadcast to all
    /// </summary>
    public async Task NotificationReceived(NotificationMessage message)
    {
        try
        {
            if (!string.IsNullOrEmpty(message.UserId))
            {
                await _connectionManager.SendToUserAsync(message.UserId, "NotificationReceived", message);
            }
            else
            {
                await _signalRClient.SendToAllAsync("NotificationReceived", message);
            }

            _logger.LogInformation("Notification sent: {Title}", message.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification: {Title}", message.Title);
        }
    }

    /// <summary>
    /// Handle client connection
    /// </summary>
    public async Task OnConnectedAsync(string userId, string connectionId)
    {
        try
        {
            await _connectionManager.AddConnectionAsync(userId, connectionId);
            _logger.LogInformation("Client connected: User {UserId}, Connection {ConnectionId}", userId, connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling client connection for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Handle client disconnection
    /// </summary>
    public async Task OnDisconnectedAsync(string userId, string connectionId)
    {
        try
        {
            await _connectionManager.RemoveConnectionAsync(connectionId);
            _logger.LogInformation("Client disconnected: User {UserId}, Connection {ConnectionId}", userId, connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling client disconnection for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Handle reconnection attempts
    /// </summary>
    public async Task OnReconnectedAsync(string userId, string connectionId)
    {
        try
        {
            var success = await _connectionManager.HandleReconnectionAsync(userId, connectionId);
            if (success)
            {
                _logger.LogInformation("Client reconnected successfully: User {UserId}, Connection {ConnectionId}", userId, connectionId);
            }
            else
            {
                _logger.LogWarning("Client reconnection failed: User {UserId}, Connection {ConnectionId}", userId, connectionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling client reconnection for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Add connection to job tracking group
    /// </summary>
    [Function("JoinJobTracking")]
    public async Task<HttpResponseData> JoinJobTracking(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<JoinTrackingRequest>(requestBody);
            
            if (request?.JobId == null)
            {
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("JobId is required");
                return badResponse;
            }

            var connectionId = GetConnectionIdFromRequest(req);
            if (connectionId != null)
            {
                await _hubContext.Groups.AddToGroupAsync(connectionId, $"job-{request.JobId}");
                _logger.LogInformation("Connection {ConnectionId} joined job tracking for {JobId}", 
                    connectionId, request.JobId);
            }

            return req.CreateResponse(System.Net.HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining job tracking");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Failed to join job tracking");
            return errorResponse;
        }
    }

    /// <summary>
    /// Remove connection from job tracking group
    /// </summary>
    [Function("LeaveJobTracking")]
    public async Task<HttpResponseData> LeaveJobTracking(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<LeaveTrackingRequest>(requestBody);
            
            if (request?.JobId == null)
            {
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("JobId is required");
                return badResponse;
            }

            var connectionId = GetConnectionIdFromRequest(req);
            if (connectionId != null)
            {
                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, $"job-{request.JobId}");
                _logger.LogInformation("Connection {ConnectionId} left job tracking for {JobId}", 
                    connectionId, request.JobId);
            }

            return req.CreateResponse(System.Net.HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving job tracking");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Failed to leave job tracking");
            return errorResponse;
        }
    }

    private string? GetUserIdFromRequest(HttpRequestData req)
    {
        // Extract user ID from JWT token or headers
        // This is a simplified implementation - in production, you'd validate the JWT
        if (req.Headers.TryGetValues("Authorization", out var authHeaders))
        {
            var authHeader = authHeaders.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                // In a real implementation, decode and validate the JWT token
                // For now, return a placeholder
                return "user-123"; // This should be extracted from the actual token
            }
        }
        
        return null;
    }

    private string? GetConnectionIdFromRequest(HttpRequestData req)
    {
        // Extract connection ID from request headers or query parameters
        if (req.Headers.TryGetValues("X-SignalR-ConnectionId", out var connectionIds))
        {
            return connectionIds.FirstOrDefault();
        }
        
        return null;
    }
}

/// <summary>
/// Request model for joining job tracking
/// </summary>
public class JoinTrackingRequest
{
    public string JobId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for leaving job tracking
/// </summary>
public class LeaveTrackingRequest
{
    public string JobId { get; set; } = string.Empty;
}