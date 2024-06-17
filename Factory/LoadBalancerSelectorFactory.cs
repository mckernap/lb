using Microsoft.Extensions.Options;
using lb.Model;
using lb.Algorithm;

namespace lb.Factory
{
    public class LoadBalancerSelectorFactory
    {
        private readonly IOptions<LoadBalanceOptions> _loadBalanceOptions;
        private readonly IEnumerable<IBalancingSelector> _loadBalancerSelectors;

        public LoadBalancerSelectorFactory(IOptions<LoadBalanceOptions> loadBalanceOptions
            , IEnumerable<IBalancingSelector> loadBalancerSelectors)
        {
            _loadBalanceOptions = loadBalanceOptions;
            _loadBalancerSelectors = loadBalancerSelectors;
        }

        public IBalancingSelector? Create()
        {
            string strategy = _loadBalanceOptions.Value.LoadBalancerSelector;

            return _loadBalancerSelectors
                .FirstOrDefault(selector =>
                {
                    return selector.GetType().Name == strategy;
                });
        }
    }
}

