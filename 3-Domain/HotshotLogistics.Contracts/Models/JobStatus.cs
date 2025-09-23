namespace HotshotLogistics.Contracts.Models
{
    /// <summary>
    /// Represents the status of a job.
    /// </summary>
    public enum JobStatus
    {
        /// <summary>
        /// Job has been created but not yet assigned to a driver.
        /// </summary>
        Pending,

        /// <summary>
        /// Job has been assigned to a driver.
        /// </summary>
        Assigned,

        /// <summary>
        /// Driver is en route to pickup location.
        /// </summary>
        EnRoute,

        /// <summary>
        /// Job is in progress (pickup completed, en route to delivery).
        /// </summary>
        InProgress,

        /// <summary>
        /// Driver is en route to delivery location.
        /// </summary>
        InTransit,

        /// <summary>
        /// Job has been completed successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// Job has been delivered successfully.
        /// </summary>
        Delivered,

        /// <summary>
        /// Job has been cancelled.
        /// </summary>
        Cancelled
    }
}
