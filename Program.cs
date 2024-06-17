using lb.Configuration;
using lb.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace lb
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                IHost host = Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config
                            .SetBasePath(AppContext.BaseDirectory)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile("configuration/serilog.json", optional: false, reloadOnChange: true)
                            .AddCommandLine(source =>
                             {
                                source.Args = args;
                                source.SwitchMappings = new Dictionary<string, string>()
                                {
                                    { "--health-check-delay", $"{nameof(LoadBalanceOptions)}:{nameof(LoadBalanceOptions.HealthCheckDelay)}" }
                                };
                             });
                    })     
                    .ConfigureServices((hostContext, services) =>
                    {

                        // Configure default options
                        var startup = new Startup(hostContext.Configuration);
                        startup.ConfigureLogging();
                        startup.ConfigureServices(services);

                        var delay = hostContext.Configuration.GetSection(nameof(LoadBalanceOptions))[nameof(LoadBalanceOptions.HealthCheckDelay)];
                        Log.Logger.Information("Running the load balancer with a health check every {delay}ms", delay);
                    })
                    .Build();
               
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting the load balancer: {ex.Message}");
            }
        }
    }
}