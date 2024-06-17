using lb.Model;
using lb.Provider;

namespace lb.Algorithm
{
    public class RoundRobinSelector : IBalancingSelector
    {
        private readonly IServerProvider _serverProvider;
        private int _currentIndex = 0;

        public RoundRobinSelector(IServerProvider serverProvider)
        {
            _serverProvider = serverProvider;
        }

        public BackendServer? Next()
        {
            if (_serverProvider.Count() != 0)
            {
                int unhealthyServerCount = 0;
                int totalServerUniverse = _serverProvider.Count();

                do
                {
                    int index = _currentIndex++ % totalServerUniverse;

                    BackendServer nextServer = _serverProvider
                        .GetByIndex(index);

                    if (nextServer.IsHealthy)
                    {
                        return nextServer; // Found one to use
                    }
                    else
                    {
                        unhealthyServerCount++;
                    }

                } while (unhealthyServerCount != totalServerUniverse);
            }
            return null;
        }
    }
}

