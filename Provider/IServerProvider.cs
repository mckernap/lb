using lb.Model;

namespace lb.Provider
{
    public interface IServerProvider
    {
        public IList<BackendServer> GetBackendServers();
        public void UpdateHealthStatus(BackendServer updatedServer);
        public int Count();
        public BackendServer GetByIndex(int index);
    }
}