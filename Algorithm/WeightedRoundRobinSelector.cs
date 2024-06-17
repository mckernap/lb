using lb.Model;
using lb.Provider;

namespace lb.Algorithm
{
    public class WeightedRoundRobinSelector : IBalancingSelector
    {
        private readonly IServerProvider _serverProvider;
        private int _currentIndex;
        private int _currentWeightCount;
        private int _totalServerCount;
        private int _sumWeights;

        public WeightedRoundRobinSelector(IServerProvider serverProvider)
        {
            _serverProvider = serverProvider;
            _currentIndex = 0;
            _currentWeightCount = 0;
            _totalServerCount = _serverProvider.Count();
            _sumWeights = _serverProvider.GetBackendServers().Sum(x => x.Weight);
        }

        private int GetNextWeightedIndex()
        {
            int currentServerIndex = _currentIndex % _totalServerCount;
            BackendServer currentServer = _serverProvider.GetByIndex(currentServerIndex);

            // Check if the current server's weight is greater than 1
            if (currentServer.Weight > 1)
            {
                if (_currentWeightCount >= currentServer.Weight)
                {
                    _currentIndex++;
                    _currentWeightCount = 0;
                    currentServerIndex = _currentIndex % _totalServerCount;
                }
                _currentWeightCount++;
            }
            else
            {
                _currentIndex++;
                _currentWeightCount = 0;
            }
        
            return currentServerIndex;
        }

        public BackendServer? Next()
        {
            if (_totalServerCount != 0)
            {
                int unhealthyServerByWeightCount = 0;

                do
                {
                    int index = GetNextWeightedIndex();

                    BackendServer nextServer = _serverProvider
                        .GetByIndex(index);

                    if (nextServer.IsHealthy)
                    {
                        return nextServer; // Found one to use
                    }
                    else
                    {
                        unhealthyServerByWeightCount += nextServer.Weight;
                    }

                } while (unhealthyServerByWeightCount < _sumWeights);
            }
            return null;
        }
    }
}

