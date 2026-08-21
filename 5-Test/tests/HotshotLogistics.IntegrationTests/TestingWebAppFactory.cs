using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace HotshotLogistics.IntegrationTests
{
    public class TestingWebAppFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            builder.ConfigureServices(_ =>
            {
                // Optionally override config/services for tests, e.g. test DB, seeded data, etc.
            });
        }
    }
}
