using lb.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace lb.Configuration
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        IEnumerable<BackendServer> GetBackendServers(IConfiguration config)
        {
            List<BackendServer>? availableServers = config
                .GetSection("BackendServers")
                .Get<List<BackendServer>>();

            return availableServers ?? Enumerable.Empty<BackendServer>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configure services here
            services.Configure<LoadBalanceOptions>(_configuration.GetSection(nameof(LoadBalanceOptions)));

            services.Configure<BackendServerOptions>(options =>
            {
                List<BackendServer> availableServers = GetBackendServers(_configuration).ToList();

                options.BackendServers = availableServers ?? new List<BackendServer>();
            });

            services
                .RegisterSingletons()
                .RegisterHostedServices();
        }

        public void ConfigureLogging()
        {
            // Configure globally shared logger
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .CreateLogger();
        }
    }
}
