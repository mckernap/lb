using lb.Algorithm;
using lb.Factory;
using lb.Model;
using lb.Provider;
using lb.tests.Mocks;
using Microsoft.Extensions.Options;
using NSubstitute;
using Serilog;
using System.Net;

namespace lb.tests.Integration
{

    [TestFixture]
    public class HealthCheckProviderTests
    {
        private IOptions<LoadBalanceOptions>? _lbOptions;
        private IServerProvider? _serverProvider;

        private ILogger _logger;

        [SetUp]
        public void Setup()
        {
            _logger = Substitute.For<ILogger>();

            _lbOptions = Options.Create(new LoadBalanceOptions
            {
                HealthCheckDelay = 1000,
                LoadBalancerSelector = "RoundRobin",
                Port = 443
            });

            _serverProvider = Substitute.For<IServerProvider>();
        }

        [TearDown]
        public void BaseTearDown()
        {

        }

        [TestCase(HttpStatusCode.NotFound, true)]
        [TestCase(HttpStatusCode.OK, false)]
        public async Task ExecuteAsync_SendsUpdateNotification(HttpStatusCode status, bool Ishealthy)
        {
            // Arrange
            var server = new BackendServer { HostName = "localhost", Port = 7076, IsHealthy = Ishealthy };
            _serverProvider?.GetBackendServers().Returns(new List<BackendServer> { server });
            var selector = new LoadBalancerSelectorFactory(_lbOptions, [new RoundRobinSelector(_serverProvider)]);

            var mockHttpMessageHandler = new MockHttpMessageHandler();
            mockHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                return Task.FromResult(new HttpResponseMessage(status));
            };

            var httpClientFactory = new HttpClientFactory(mockHttpMessageHandler);

            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            var mockhealthCheckProvider = new MockHealthCheckProvider(_lbOptions, _serverProvider, httpClientFactory, _logger);

            // Act
            Task healthCheckTask = Task.Run(() =>
            {
                var healthCheck = mockhealthCheckProvider.ExecuteAsync(token);

            });

            Task LoadBalancerTask = Task.Run(() =>
            {
                var lb = new LoadBalancer(_lbOptions, _serverProvider, selector, _logger);
                mockhealthCheckProvider.Subscribe(lb);
            });

            await Task.WhenAll(LoadBalancerTask, healthCheckTask);

            await Task.Delay(5000); // Let it run for a bit
            source.Cancel();

            BackendServer? notifiedServerUpdate = _serverProvider
                .GetBackendServers()
                .FirstOrDefault();

            // Assert
            Assert.That(notifiedServerUpdate, Is.Not.Null);
            Assert.That(notifiedServerUpdate.IsHealthy, Is.Not.EqualTo(Ishealthy));
        }
    }
}
