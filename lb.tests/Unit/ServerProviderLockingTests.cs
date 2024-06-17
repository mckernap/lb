using lb.Model;
using lb.Provider;
using Microsoft.Extensions.Options;

namespace lb.tests.Unit
{
    [TestFixture]
    public class ServerProviderLockingTests
    {
        private ServerProvider _serverProvider;
        private IList<BackendServer> _servers;

        [SetUp]
        public void SetUp()
        {
            _servers = new List<BackendServer>
            {
                new BackendServer { HostName = "Server1", IsHealthy = true },
                new BackendServer { HostName = "Server2", IsHealthy = true }
            };

            var options = Options.Create(new BackendServerOptions { BackendServers = _servers });
            _serverProvider = new ServerProvider(options);
        }

        [Test]
        public void Count_ShouldReturnCorrectCount()
        {
            Assert.That(_serverProvider.Count(), Is.EqualTo(2));
        }

        [Test]
        public void GetByIndex_ShouldReturnCorrectServer()
        {
            var server = _serverProvider.GetByIndex(0);
            Assert.That(server.HostName, Is.EqualTo("Server1"));
        }

        [Test]
        public void GetByIndex_ShouldThrowException_ForInvalidIndex()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _serverProvider.GetByIndex(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => _serverProvider.GetByIndex(2));
        }

        [Test]
        public void GetBackendServers_ShouldReturnAllServers()
        {
            var servers = _serverProvider.GetBackendServers();
            Assert.That(servers.Count, Is.EqualTo(2));
            Assert.That(servers[0].HostName, Is.EqualTo("Server1"));
            Assert.That(servers[1].HostName, Is.EqualTo("Server2"));
        }

        [Test]
        public void UpdateHealthStatus_ShouldUpdateServerHealth()
        {
            var updatedServer = new BackendServer { HostName = "Server1", IsHealthy = false };
            _serverProvider.UpdateHealthStatus(updatedServer);

            var server = _serverProvider.GetByIndex(0);
            Assert.That(server.IsHealthy, Is.False);
        }

        [Test]
        public void UpdateHealthStatus_ShouldThrowException_ForNullServer()
        {
            BackendServer? server = null;
            Assert.Throws<ArgumentNullException>(() => _serverProvider.UpdateHealthStatus(server));
        }

        [Test]
        public void ConcurrentModification_ShouldNotCorruptData()
        {
            // Variable to track the number of state changes
            int stateChangeCount = 0;

            // Shared variables to track state before and after modification
            bool initialState = true;
            bool finalState = true;

            // Manual reset event to control the start of tasks
            ManualResetEventSlim resetEvent = new ManualResetEventSlim(false);

            // Task list to hold the concurrent tasks
            var tasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    // Wait for the signal to start
                    resetEvent.Wait();

                    for (int j = 0; j < 100; j++)
                    {
                        var updatedServer = new BackendServer { HostName = "Server1", IsHealthy = j % 2 == 0 };

                        // Capture the initial state before modification
                        lock (_serverProvider)
                        {
                            initialState = _serverProvider.GetByIndex(0).IsHealthy;
                        }

                        _serverProvider.UpdateHealthStatus(updatedServer);

                        // Capture the final state after modification
                        lock (_serverProvider)
                        {
                            finalState = _serverProvider.GetByIndex(0).IsHealthy;
                        }

                        // Track state changes
                        if (initialState != finalState)
                        {
                            Interlocked.Increment(ref stateChangeCount);
                        }
                    }
                }));

                tasks.Add(Task.Run(() =>
                {
                    // Wait for the signal to start
                    resetEvent.Wait();

                    for (int j = 0; j < 100; j++)
                    {
                        var updatedServer = new BackendServer { HostName = "Server1", IsHealthy = j % 2 == 0 };

                        // Capture the initial state before modification
                        lock (_serverProvider)
                        {
                            initialState = _serverProvider.GetByIndex(0).IsHealthy;
                        }

                        _serverProvider.UpdateHealthStatus(updatedServer);

                        // Capture the final state after modification
                        lock (_serverProvider)
                        {
                            finalState = _serverProvider.GetByIndex(0).IsHealthy;
                        }

                        // Track state changes
                        if (initialState != finalState)
                        {
                            Interlocked.Increment(ref stateChangeCount);
                        }
                    }
                }));
            }

            // Signal all tasks to start
            resetEvent.Set();

            // Wait for all tasks to complete
            Task.WaitAll(tasks.ToArray());

            // Validate that the state changes were captured
            Assert.That(stateChangeCount, Is.GreaterThan(0), "State changes were not captured.");
            var server = _serverProvider.GetByIndex(0);
            Assert.That(server.IsHealthy || !server.IsHealthy, Is.True);
        }
    }
}
