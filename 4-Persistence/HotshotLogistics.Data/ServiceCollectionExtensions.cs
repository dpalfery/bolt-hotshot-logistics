// <copyright file="ServiceCollectionExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using HotshotLogistics.Contracts.Repositories;
using HotshotLogistics.Contracts.Services;
using HotshotLogistics.Data.Repositories;
using HotshotLogistics.Data.Services;
using HotshotLogistics.Application.Services;
using HotshotLogistics.Domain.Repositories;
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
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<ILocationTrackingRepository, LocationTrackingRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            return services;
        }

        public static IServiceCollection AddMappingServices(this IServiceCollection services)
        {
            services.AddHttpClient("MappingService");
            services.AddTransient<IMappingService, MockMappingService>();
            services.AddTransient<IMappingService, AzureMapsService>();
            services.AddTransient<IMappingService, GoogleMapsService>();
            return services;
        }

        public static IServiceCollection AddCommunicationServices(this IServiceCollection services)
        {
            services.AddTransient<ICommunicationService, TwilioSmsService>();
            services.AddTransient<ICommunicationService, SendGridEmailService>();
            services.AddTransient<ICommunicationService, AzureNotificationHubService>();
            services.AddTransient<ICommunicationServiceFactory, CommunicationServiceFactory>();
            return services;
        }
    }
}
