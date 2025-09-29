using HotshotLogistics.Contracts.Models;

namespace HotshotLogistics.Contracts.Repositories;

public interface IJobRepository
{
    /// <summary>
    /// Gets a job by its identifier.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <returns>The job if found, null otherwise.</returns>
    Task<IJob?> GetByIdAsync(object id);

    /// <summary>
    /// Gets all jobs.
    /// </summary>
    /// <returns>A list of all jobs.</returns>
    Task<IEnumerable<IJob>> GetAllAsync();

    /// <summary>
    /// Adds a new job.
    /// </summary>
    /// <param name="entity">The job to add.</param>
    /// <returns>The added job.</returns>
    Task<IJob> AddAsync(IJob entity);

    /// <summary>
    /// Updates an existing job.
    /// </summary>
    /// <param name="entity">The job to update.</param>
    /// <returns>The updated job.</returns>
    Task<IJob> UpdateAsync(IJob entity);

    /// <summary>
    /// Deletes a job by its identifier.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <returns>True if the job was deleted, false otherwise.</returns>
    Task<bool> DeleteAsync(object id);

    /// <summary>
    /// Checks if a job exists by its identifier.
    /// </summary>
    /// <param name="id">The job identifier.</param>
    /// <returns>True if the job exists, false otherwise.</returns>
    Task<bool> ExistsAsync(object id);

    Task<IEnumerable<IJob>> GetJobsAsync(CancellationToken cancellationToken = default);

    Task<IJob?> GetJobByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IJob> CreateJobAsync(IJob job, CancellationToken cancellationToken = default);
    Task<IJob?> UpdateJobAsync(string id, IJob jobDetails, CancellationToken cancellationToken = default);
    Task<bool> DeleteJobAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets jobs with filtering and pagination support.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <param name="pagination">The pagination parameters.</param>
    /// <param name="sort">The sort parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result of jobs.</returns>
    Task<PagedResult<IJob>> GetJobsAsync(
        JobFilter? filter = null,
        PaginationParameters? pagination = null,
        SortParameters? sort = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets jobs by status.
    /// </summary>
    /// <param name="status">The job status.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of jobs with the specified status.</returns>
    Task<IEnumerable<IJob>> GetJobsByStatusAsync(JobStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets jobs assigned to a specific driver.
    /// </summary>
    /// <param name="driverId">The driver ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of jobs assigned to the driver.</returns>
    Task<IEnumerable<IJob>> GetJobsByDriverAsync(int driverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets jobs for a specific customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of jobs for the customer.</returns>
    Task<IEnumerable<IJob>> GetJobsByCustomerAsync(string customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets overdue jobs (past estimated delivery time).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of overdue jobs.</returns>
    Task<IEnumerable<IJob>> GetOverdueJobsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of jobs matching the filter criteria.
    /// </summary>
    /// <param name="filter">The filter criteria.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The total count of matching jobs.</returns>
    Task<int> GetJobCountAsync(JobFilter? filter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets jobs assigned to a specific driver by driver ID.
    /// </summary>
    /// <param name="driverId">The driver ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of jobs assigned to the driver.</returns>
    Task<IEnumerable<IJob>> GetByDriverIdAsync(int driverId, CancellationToken cancellationToken = default);
}
