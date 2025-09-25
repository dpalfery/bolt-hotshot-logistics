// <copyright file="ServiceCollectionExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using HotshotLogistics.Contracts.Repositories;
using HotshotLogistics.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HotshotLogistics.Data
{
    /// <summary>
    /// Extension methods for configuring services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Hotshot repositories to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddHotshotRepositories(this IServiceCollection services)
        {
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IDriverRepository, DriverRepository>();
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<IJobAssignmentRepository, JobAssignmentRepository>();
            return services;
        }
    }
}
