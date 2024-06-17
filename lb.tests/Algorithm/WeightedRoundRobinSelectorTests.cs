using lb.Algorithm;
using lb.Model;
using lb.Provider;
using Microsoft.Extensions.Options;

namespace lb.tests.Algorithm
{
    [TestFixture]
    public class WeightedRoundRobinSelectorTests
    {
        [Test]
        public void Next_NoServers_ReturnsNull()
        {
            // Arrange
            var mockOptions = Options.Create(new BackendServerOptions
            {
                BackendServers = Enumerable.Empty<BackendServer>().ToList()
            });

            var mockServerProvider = new ServerProvider(mockOptions);
            var selector = new WeightedRoundRobinSelector(mockServerProvider);

            // Act
            var nextServer = selector.Next();

            // Assert
            Assert.That(nextServer, Is.Null);
        }

        [Test]
        public void Next_SingleHealthyServer_ReturnsServer()
        {
            // Arrange
            var mockOptions = Options.Create(new BackendServerOptions
            {
                BackendServers = new List<BackendServer>
                {
                    new BackendServer { HostName = "localhost", Port = 7076, IsHealthy = true, Weight = 1 }
                }
            });

            var mockServerProvider = new ServerProvider(mockOptions);
            var selector = new WeightedRoundRobinSelector(mockServerProvider);

            // Act
            var nextServer = selector.Next();

            // Assert
            Assert.That(nextServer, Is.Not.Null);
            Assert.That(nextServer?.IsHealthy, Is.True);
        }

        [Test]
        public void Next_MultipleHealthyServers_ReturnsHealthyServerRoundRobin()
        {
            // Arrange
            var mockOptions = Options.Create(new BackendServerOptions
            {
                BackendServers = new List<BackendServer>
                {
                    new BackendServer { HostName = "localhost", Port = 7076, IsHealthy = true, Weight = 10 },
                    new BackendServer { HostName = "localhost", Port = 7077, IsHealthy = true, Weight = 15 }
                }
            });

            var mockServerProvider = new ServerProvider(mockOptions);
            var selector = new WeightedRoundRobinSelector(mockServerProvider);

            // Act
            var firstServer = selector.Next();
            var secondServer = selector.Next();

            // Assert
            Assert.That(firstServer, Is.Not.Null);
            Assert.That(secondServer, Is.Not.Null);

            Assert.That(firstServer.IsHealthy, Is.True);
            Assert.That(secondServer.IsHealthy, Is.True);
        }

        [Test]
        public void Next_SingleUnhealthyServer_ReturnsNull()
        {
            // Arrange
            var mockOptions = Options.Create(new BackendServerOptions
            {
                BackendServers = new List<BackendServer>
                {
                    new BackendServer { HostName = "localhost", Port = 7076, IsHealthy = false, Weight = 3 }
                }
            });

            var mockServerProvider = new ServerProvider(mockOptions);
            var selector = new WeightedRoundRobinSelector(mockServerProvider);

            // Act
            var nextServer = selector.Next();

            // Assert
            Assert.That(nextServer, Is.Null);
        }

        [Test]
        public void Next_MultipleUnhealthyServers_ReturnsNullAfterAllWeightsExhausted()
        {
            // Arrange
            var mockOptions = Options.Create(new BackendServerOptions
            {
                BackendServers = new List<BackendServer>
                {
                    new BackendServer { HostName = "localhost", Port = 7076, IsHealthy = false, Weight = 2 },
                    new BackendServer { HostName = "localhost", Port = 7077, IsHealthy = false, Weight = 1 }
                }
            });

            var mockServerProvider = new ServerProvider(mockOptions);
            var selector = new WeightedRoundRobinSelector(mockServerProvider);

            // Act
            BackendServer? firstServer = selector.Next();
            BackendServer? secondServer = selector.Next();

            // Assert
            Assert.That(firstServer, Is.Null);
            Assert.That(secondServer, Is.Null);
        }

        [Test]
        public void Next_MixedServers_ReturnsHealthyServerRoundRobin()
        {
            // Arrange
            var mockOptions = Options.Create(new BackendServerOptions
            {
                BackendServers = new List<BackendServer>
                {
                    new BackendServer { HostName = "localhost", Port = 7076, IsHealthy = true, Weight = 1 },
                    new BackendServer { HostName = "localhost", Port = 7077, IsHealthy = false, Weight = 2 },
                    new BackendServer { HostName = "localhost", Port = 7078, IsHealthy = true, Weight = 5 }
                }
            });

            var mockServerProvider = new ServerProvider(mockOptions);
            var selector = new WeightedRoundRobinSelector(mockServerProvider);

            // Act
            var firstServer = selector.Next();
            var thirdServerWeight_One = selector.Next();
            var thirdServerWeight_Two = selector.Next();
            var thirdServerWeight_Three = selector.Next();
            var thirdServerWeight_Four = selector.Next();
            var thirdServerWeight_Five = selector.Next();

            int? totalWeight = thirdServerWeight_One?.Weight +
                    thirdServerWeight_Two?.Weight +
                    thirdServerWeight_Three?.Weight +
                    thirdServerWeight_Four?.Weight +
                    thirdServerWeight_Five?.Weight;

            // Assert
            Assert.That(firstServer, Is.Not.Null);
            Assert.That(totalWeight, Is.Not.Null);
            Assert.That(totalWeight == 25, Is.True);
        }
    }
}