using lb.Algorithm;
using lb.Factory;
using lb.Provider;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace lb.Configuration
{
    public static class ServiceRegistrationExtensions
    {
        public static IServiceCollection RegisterSingletons(this IServiceCollection services)
        {
            services.AddSingleton(Log.Logger);
            services.AddSingleton<IServerProvider, ServerProvider>();
            services.AddSingleton<IBalancingSelector, RoundRobinSelector>();
            services.AddSingleton<IBalancingSelector, WeightedRoundRobinSelector>();
            services.AddSingleton<LoadBalancerSelectorFactory>();

            services.AddSingleton<Factory.IHttpClientFactory, HttpClientFactory>();

            services.AddSingleton<HealthCheckProvider>();
            services.AddSingleton<LoadBalancer>();

            return services;
        }

        public static IServiceCollection RegisterHostedServices(this IServiceCollection services)
        {
            services.AddHostedService(sp =>
            {
                var provider = sp.GetRequiredService<HealthCheckProvider>();
                return provider;
            });

            services.AddHostedService(sp =>
            {
                var healthCheckProvider = sp.GetRequiredService<HealthCheckProvider>();
                var loadBalancer = sp.GetRequiredService<LoadBalancer>();
                healthCheckProvider.Subscribe(loadBalancer);
                return loadBalancer;
            });

            return services;
        }
    }
}
