using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;
using Serilog;
using lb.Model;
using lb.Provider;
using lb.Algorithm;
using lb.Factory;

namespace lb
{
    public class LoadBalancer : BackgroundService, IHealthCheckObserver
    {
        private readonly ILogger _logger = Log.ForContext<LoadBalancer>();

        private readonly LoadBalanceOptions _loadBalanceOptions;
        private readonly IServerProvider _serverProvider;
        private readonly LoadBalancerSelectorFactory _factory;
        private readonly IBalancingSelector _selector;

        private readonly object _lock = new object();

        public LoadBalancer(IOptions<LoadBalanceOptions> loadBalanceOptions
            , IServerProvider serverProvider
            , LoadBalancerSelectorFactory factory
            , ILogger logger)
        {
            _loadBalanceOptions = loadBalanceOptions.Value;
            _serverProvider = serverProvider;
            _factory = factory;
            _logger = logger;

            _selector = _factory.Create()!;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, _loadBalanceOptions.Port);

            _logger.Information("Load Balancer is listening on port {Port}", _loadBalanceOptions.Port);

            listener.Start();

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    TcpClient? client = null;
                    try
                    {
                        client = await listener.AcceptTcpClientAsync();

                        BackendServer? server = GetNextAvailableServer();
                        if (server != null)
                        {
                            await Task.Run(() => RoutRequestToServerAsync(client, server));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Error accepting client: {ex}", ex.Message);
                        continue; // Continue to the next iteration of the loop
                    }
                    finally
                    {
                        client?.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Server error: {ex}", ex);
            }
            finally
            {
                listener.Stop();
            }
        }

        private BackendServer? GetNextAvailableServer()
        {
            lock (_lock)
            {
                return _selector.Next();
            }
        }

        private async Task RoutRequestToServerAsync(TcpClient client, BackendServer server)
        {

            using (NetworkStream clientStream = client.GetStream())
            using (StreamWriter writer = new StreamWriter(clientStream))
            {
                StreamReader reader = new StreamReader(clientStream);
                string? request = await reader.ReadLineAsync();
                if (request != null)
                {
                    _logger.Information("Received request from: {request}", request);
                }
                else
                {
                    _logger.Error("Unable to read client data stream.");
                }
                // Pass the message to the backend service
                HttpClient httpBackendService = new HttpClient();

                try
                {
                    // string data2 = "\"abcdefgh\"";

                    UriBuilder uriBuilder = new UriBuilder
                    {
                        Scheme = Uri.UriSchemeHttp,
                        Host = server.HostName,
                        Port = server.Port
                    };

                    //var content = new StringContent(data2, Encoding.UTF8, "application/json");
                    //var backendResponse = await httpClient.PostAsync(new Uri(backendServiceUrl + "/SendMessage"), content);
                    //var backendResponse = await httpClient.GetAsync(new Uri(backendServiceUrl + "/GetMessage"));
                    var backendResponse = await httpBackendService.GetAsync(uriBuilder.Uri);

                    if (backendResponse.IsSuccessStatusCode)
                    {

                        // Read the response from the backend service
                        string backendResponseContent = await backendResponse.Content.ReadAsStringAsync();
                        string backendStatus = $"HTTP/{backendResponse.Version} {(int)backendResponse.StatusCode} {backendResponse.StatusCode}";
                        string result = $"\nResponse from server: {backendStatus}\n\n{backendResponseContent}";
                        _logger.Information(result);
                        // Respond to the client with the backend service response
                        await writer.WriteLineAsync(result);
                        await writer.FlushAsync();
                    }
                    else
                    {
                        _logger.Information("Backend Service Error: {StatusCode}", backendResponse.StatusCode);
                        await writer.WriteLineAsync($"Backend Service Error: {backendResponse.StatusCode}");
                        await writer.FlushAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error handling client request: {ex}", ex.Message);
                }

            }
        }

        public void Update(BackendServer server)
        {
            _serverProvider.UpdateHealthStatus(server);
        }
    }
}

