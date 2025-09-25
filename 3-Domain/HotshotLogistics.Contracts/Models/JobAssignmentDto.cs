namespace HotshotLogistics.Contracts.Models;

using HotshotLogistics.Contracts.Models;

/// <summary>
/// Data transfer object for JobAssignment.
/// </summary>
public class JobAssignmentDto
{
    /// <inheritdoc/>
    public string Id { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string JobId { get; set; } = string.Empty;

    /// <inheritdoc/>
    public int DriverId { get; set; }

    /// <inheritdoc/>
    public DateTime AssignedAt { get; set; }

    /// <inheritdoc/>
    public JobAssignmentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the driver details.
    /// </summary>
    public DriverDto? Driver { get; set; }

    /// <summary>
    /// Gets or sets the job details.
    /// </summary>
    public JobDto? Job { get; set; }
}
