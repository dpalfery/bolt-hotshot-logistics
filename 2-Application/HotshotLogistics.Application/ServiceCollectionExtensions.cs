using HotshotLogistics.Application.Services;
using HotshotLogistics.Application.Validators;
using HotshotLogistics.Contracts.Services;
using HotshotLogistics.Contracts.Hubs;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace HotshotLogistics.Application;

/// <summary>
/// Extension methods for setting up services in the application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IConnectionManagerService, ConnectionManagerService>();
        services.AddScoped<DriverService, DriverService>();
        services.AddScoped<JobService, JobService>();
        services.AddScoped<JobAssignmentService, JobAssignmentService>();
        services.AddScoped<IRealtimeService, RealtimeService>();
        services.AddScoped<ISignalRClientWrapper, SignalRClientWrapper>();
        services.AddScoped<UserProfileService, UserProfileService>();

        // Register billing and payment services
        services.AddScoped<IBillingService, BillingService>();
        services.AddScoped<CustomerService, CustomerService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ITrackingService, TrackingService>();

        /* Register payment processors */
        services.AddScoped<StripePaymentProcessor>();
        services.AddScoped<PayPalPaymentProcessor>();
        services.AddScoped<IPaymentProcessorFactory, PaymentProcessorFactory>();

        // Register mapping services
        services.AddSingleton<MappingServiceFactory>();
        services.AddScoped<IMappingService>(sp =>
        {
            var factory = sp.GetRequiredService<MappingServiceFactory>();
            return factory.CreateMappingService();
        });

        // Register validators
        services.AddValidatorsFromAssemblyContaining<CreateJobValidator>();

        return services;
    }
}
