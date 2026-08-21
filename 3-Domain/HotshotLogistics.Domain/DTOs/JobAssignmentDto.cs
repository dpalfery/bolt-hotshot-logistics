using HotshotLogistics.Core.Enums;
using HotshotLogistics.Domain.Entities;

namespace HotshotLogistics.Domain.DTOs
{
    /// <summary>
    ///     Data transfer object for JobAssignment.
    /// </summary>
    public class JobAssignmentDto
    {
        /// <summary>
        ///     Gets or sets the assignment identifier.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the job identifier.
        /// </summary>
        public string JobId { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the driver identifier.
        /// </summary>
        public int DriverId { get; set; }

        /// <summary>
        ///     Gets or sets the assignment timestamp.
        /// </summary>
        public DateTime AssignedAt { get; set; }

        /// <summary>
        ///     Gets or sets the status of the job assignment.
        /// </summary>
        public JobAssignmentStatus Status { get; set; }

        /// <summary>
        ///     Gets or sets the driver details.
        /// </summary>
        public DriverDto? Driver { get; set; }

        /// <summary>
        ///     Gets or sets the job details.
        /// </summary>
        public Job? Job { get; set; }
    }
}
