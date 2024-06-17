using lb.Model;
using Microsoft.Extensions.Options;

namespace lb.Provider
{
    public class ServerProvider : IServerProvider
    {
        private readonly IList<BackendServer> _serverUniverse;
        private readonly object _lock = new object();

        public ServerProvider(IOptions<BackendServerOptions> backendServerOptions)
        {
            _serverUniverse = backendServerOptions.Value.BackendServers;
        }

        public int Count() => _serverUniverse.Count();

        public BackendServer GetByIndex(int index)
        {
            lock (_lock)
            {
                if (index < 0 || index >= _serverUniverse.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }

                return _serverUniverse.ElementAt(index);
            }
        }

        public IList<BackendServer> GetBackendServers()
        {
            lock (_lock)
            {
                return _serverUniverse
                    .Select(server => new BackendServer(server))
                    .ToList();
            }
        }

        public void UpdateHealthStatus(BackendServer updatedServer)
        {
            if (updatedServer == null)
            {
                throw new ArgumentNullException(nameof(updatedServer));
            }

            lock (_lock)
            {
                BackendServer? server = _serverUniverse.FirstOrDefault(x => x.Equals(updatedServer));

                if (server != null)
                { 
                    if (server.IsHealthy != updatedServer.IsHealthy)
                    {
                        // Update the IsHealthy property
                        server.IsHealthy = updatedServer.IsHealthy;
                    }
                }
            }
        }
    }
}
