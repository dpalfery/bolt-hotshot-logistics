using HotshotLogistics.Contracts.Models;

namespace HotshotLogistics.Contracts.Repositories
{
    public interface IDriverRepository
    {
        /// <summary>
        /// Gets a driver by their identifier.
        /// </summary>
        /// <param name="id">The driver identifier.</param>
        /// <returns>The driver if found, null otherwise.</returns>
        Task<IDriver?> GetByIdAsync(object id);

        /// <summary>
        /// Gets all drivers.
        /// </summary>
        /// <returns>A list of all drivers.</returns>
        Task<IEnumerable<IDriver>> GetAllAsync();

        /// <summary>
        /// Adds a new driver.
        /// </summary>
        /// <param name="entity">The driver to add.</param>
        /// <returns>The added driver.</returns>
        Task<IDriver> AddAsync(IDriver entity);

        /// <summary>
        /// Updates an existing driver.
        /// </summary>
        /// <param name="entity">The driver to update.</param>
        /// <returns>The updated driver.</returns>
        Task<IDriver> UpdateAsync(IDriver entity);

        /// <summary>
        /// Deletes a driver by their identifier.
        /// </summary>
        /// <param name="id">The driver identifier.</param>
        /// <returns>True if the driver was deleted, false otherwise.</returns>
        Task<bool> DeleteAsync(object id);

        /// <summary>
        /// Checks if a driver exists by their identifier.
        /// </summary>
        /// <param name="id">The driver identifier.</param>
        /// <returns>True if the driver exists, false otherwise.</returns>
        Task<bool> ExistsAsync(object id);

        Task<IEnumerable<IDriver>> GetDriversAsync(CancellationToken cancellationToken = default);
        Task<IDriver?> GetDriverByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IDriver> CreateDriverAsync(IDriver driver, CancellationToken cancellationToken = default);
        Task DeleteDriverAsync(int id, CancellationToken cancellationToken = default);
        Task<IDriver> UpdateDriverAsync(IDriver driver, CancellationToken cancellationToken = default);
    }
}
