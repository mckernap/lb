using lb.Model;
using lb.Provider;
using Microsoft.Extensions.Options;
using Serilog;

namespace lb.tests.Mocks
{
    public class MockHealthCheckProvider : HealthCheckProvider
    {
        public MockHealthCheckProvider(IOptions<LoadBalanceOptions> options
            , IServerProvider serverProvider
            , Factory.IHttpClientFactory httpClientFactory
            , ILogger logger)
            : base(options, serverProvider, httpClientFactory, logger)
        {
        }

        public new Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return base.ExecuteAsync(stoppingToken);
        }
    }
}
