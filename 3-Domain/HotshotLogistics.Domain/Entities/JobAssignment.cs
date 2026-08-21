using HotshotLogistics.Core.Enums;

namespace HotshotLogistics.Domain.Entities
{
    /// <summary>
    ///     Represents a job assignment to a driver in the system.
    /// </summary>
    public class JobAssignment
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JobAssignment" /> class.
        /// </summary>
        public JobAssignment()
        {
            if (AssignedAt == default)
            {
                AssignedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        ///     Gets or sets the assignment identifier.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        ///     Gets or sets the assigned job identifier.
        /// </summary>
        public string JobId { get; set; } = string.Empty;

        /// <summary>
        ///     Navigation property for the assigned job.
        /// </summary>
        public Job? Job { get; set; }

        /// <summary>
        ///     Gets or sets the assigned driver identifier.
        /// </summary>
        public int DriverId { get; set; }

        /// <summary>
        ///     Navigation property for the assigned driver.
        /// </summary>
        public Driver? Driver { get; set; }

        /// <summary>
        ///     Gets or sets the assignment timestamp.
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        ///     Gets or sets the assignment status.
        /// </summary>
        public JobAssignmentStatus Status { get; set; } = JobAssignmentStatus.Active;

        /// <summary>
        ///     Gets or sets when the assignment was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
