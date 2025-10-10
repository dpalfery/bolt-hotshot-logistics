using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

public class MappingServiceFactory : IMappingServiceFactory
{
    private readonly IConfiguration configuration;
    private readonly ILogger<MappingServiceFactory> logger;
    private readonly IDictionary<string, IMappingService> mappingServices;

    public MappingServiceFactory(
        IConfiguration configuration,
        ILogger<MappingServiceFactory> logger,
        IDictionary<string, IMappingService> mappingServices)
    {
        this.configuration = configuration;
        this.logger = logger;
        this.mappingServices = mappingServices;
    }

    public IMappingService CreateMappingService()
    {
        // Example logic: return a default mapping service
        // Replace with your actual factory logic as needed
        return mappingServices.Values.FirstOrDefault();
    }
}
