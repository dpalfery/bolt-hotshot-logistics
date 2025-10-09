using HotshotLogistics.Contracts.Models;

namespace HotshotLogistics.Contracts.Services;

public interface IDriverService
{
    Task<IEnumerable<IDriver>> GetDriversAsync();
    Task<IDriver?> GetDriverByIdAsync(int id);
    Task<IDriver> CreateDriverAsync(IDriver driver);
    Task<IDriver> UpdateDriverAsync(IDriver driver);
    Task<bool> DeleteDriverAsync(int id);
}




