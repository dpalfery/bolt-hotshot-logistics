// <copyright file="DriverService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Application.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HotshotLogistics.Contracts.Models;
    using HotshotLogistics.Contracts.Repositories;
    using HotshotLogistics.Contracts.Services;

    /// <summary>
    /// Service for managing drivers.
    /// </summary>
    public class DriverService : IDriverService
    {
        private readonly IDriverRepository driverRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DriverService"/> class.
        /// </summary>
        /// <param name="driverRepository">The driver repository.</param>
        public DriverService(IDriverRepository driverRepository)
        {
            this.driverRepository = driverRepository;
        }

        /// <inheritdoc/>
        public Task<IDriver> CreateDriverAsync(IDriver driver)
        {

            return this.driverRepository.CreateDriverAsync(driver);

        }

        /// <inheritdoc/>
        public Task<IDriver?> GetDriverByIdAsync(int id)
        {
            return this.driverRepository.GetDriverByIdAsync(id);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<IDriver>> GetDriversAsync()
        {

            return this.driverRepository.GetDriversAsync();

        }
    }
}
