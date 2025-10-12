namespace HotshotLogistics.Domain.DTOs;

/// <summary>
/// Represents a summary of job counts by status.
/// </summary>
public class JobStatusSummaryDto
{
    /// <summary>
    /// Gets or sets the count of jobs with Pending status.
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// Gets or sets the count of jobs with Assigned status.
    /// </summary>
    public int AssignedCount { get; set; }

    /// <summary>
    /// Gets or sets the count of jobs with EnRoute status.
    /// </summary>
    public int EnRouteCount { get; set; }

    /// <summary>
    /// Gets or sets the count of jobs with Received status.
    /// </summary>
    public int ReceivedCount { get; set; }
}