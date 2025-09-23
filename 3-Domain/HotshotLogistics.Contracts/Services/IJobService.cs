using HotshotLogistics.Contracts.Models;

namespace HotshotLogistics.Contracts.Services;

public interface IJobService
{
    Task<IEnumerable<IJob>> GetJobsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets jobs with filtering, pagination, and sorting support.
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
    
    Task<IJob?> GetJobByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IJob> CreateJobAsync(IJob job, CancellationToken cancellationToken = default);
    Task<IJob?> UpdateJobAsync(string id, IJob jobDetails, CancellationToken cancellationToken = default);
    Task<bool> DeleteJobAsync(string id, CancellationToken cancellationToken = default);
    
    // Job lifecycle management methods
    Task<IJob> AssignDriverAsync(string jobId, int driverId, CancellationToken cancellationToken = default);
    Task<IJob> UpdateJobStatusAsync(string jobId, JobStatus status, CancellationToken cancellationToken = default);
    Task<bool> ValidateJobAsync(IJob job, CancellationToken cancellationToken = default);
    Task<bool> IsDriverAvailableAsync(int driverId, DateTime startTime, DateTime? endTime = null, CancellationToken cancellationToken = default);
}
