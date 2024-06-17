using lb.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace lb.Provider
{

    public class HealthCheckProvider : BackgroundService, IHealthCheckSubject
    {
        private readonly ILogger _logger = Log.ForContext<HealthCheckProvider>();

        private readonly LoadBalanceOptions _loadBalanceOptions;
        private readonly IServerProvider _serverProvider;
        private readonly Factory.IHttpClientFactory _httpClientFactory;
        private List<IHealthCheckObserver> _observers = new List<IHealthCheckObserver>();

        public HealthCheckProvider(IOptions<LoadBalanceOptions> loadBalanceOptions
            , IServerProvider serverProvider
            , Factory.IHttpClientFactory httpClientFactory
            , ILogger logger)
        {
            _loadBalanceOptions = loadBalanceOptions.Value;
            _serverProvider = serverProvider;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (BackendServer server in _serverProvider.GetBackendServers())
                {
                    bool isHealthy = await CheckServerHealth(server);
                    if (isHealthy)
                    {
                        server.IsHealthy = true;
                    }
                    else
                    {
                        server.IsHealthy = false;
                        _logger.Warning("{HostName}:{Port} is currently NOT available."
                            , server.HostName
                            , server.Port);
                    }

                    Notify(server);

                }
                await Task.Delay(_loadBalanceOptions.HealthCheckDelay, stoppingToken);
            }
        }

        public void Subscribe(IHealthCheckObserver observer)
        {
            _observers.Add(observer);
        }

        public void Unsubscribe(IHealthCheckObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify(BackendServer server)
        {
            foreach (var observer in _observers)
            {
                observer.Update(server);
            }
        }

        private async Task<bool> CheckServerHealth(BackendServer server)
        {
            bool result = false;

            try
            {
                using (HttpClient httpBackendService = _httpClientFactory.CreateClient())
                {
                    UriBuilder uriBuilder = new UriBuilder
                    {
                        Scheme = Uri.UriSchemeHttp,
                        Host = server.HostName,
                        Port = server.Port
                    };

                    HttpResponseMessage backendResponse = await httpBackendService.GetAsync(uriBuilder.Uri);

                    if (backendResponse.IsSuccessStatusCode)
                        result = true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Health check failed for {HostName}:{Port}.  Exception: {ex}"
                    , server.HostName
                    , server.Port
                    , ex.Message);
            }

            return result;
        }
    }
}
