// <copyright file="DriverService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using HotshotLogistics.Contracts.Repositories;
using HotshotLogistics.Contracts.Services;
using HotshotLogistics.Domain.Entities;

namespace HotshotLogistics.Application.Services
{
    /// <summary>
    ///     Service for managing drivers.
    /// </summary>
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository _driverRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DriverService" /> class.
        /// </summary>
        /// <param name="driverRepository">The driver repository.</param>
        public DriverService(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }

        /// <inheritdoc />
        public Task<Driver> CreateDriverAsync(Driver driver)
        {
            return _driverRepository.CreateDriverAsync(driver);
        }

        /// <inheritdoc />
        public Task<Driver?> GetDriverByIdAsync(int id)
        {
            return _driverRepository.GetDriverByIdAsync(id);
        }

        /// <inheritdoc />
        public Task<IEnumerable<Driver>> GetDriversAsync()
        {
            return _driverRepository.GetDriversAsync();
        }

        public Task<Driver> UpdateDriverAsync(Driver driver)
        {
            return _driverRepository.UpdateDriverAsync(driver);
        }

        public Task<bool> DeleteDriverAsync(int id)
        {
            return _driverRepository.DeleteDriverAsync(id);
        }
    }
}
