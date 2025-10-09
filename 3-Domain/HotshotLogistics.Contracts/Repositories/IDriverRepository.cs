using HotshotLogistics.Contracts.Models;

namespace HotshotLogistics.Contracts.Repositories
{
    public interface IDriverRepository
    {
        Task<IEnumerable<IDriver>> GetDriversAsync(CancellationToken cancellationToken = default);
        Task<IDriver?> GetDriverByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IDriver> CreateDriverAsync(IDriver driver, CancellationToken cancellationToken = default);
        Task<bool> DeleteDriverAsync(int id, CancellationToken cancellationToken = default);
        Task<IDriver> UpdateDriverAsync(IDriver driver, CancellationToken cancellationToken = default);
    }
}




