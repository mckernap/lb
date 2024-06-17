namespace lb.Model
{
    public class LoadBalanceOptions
    {
        public int HealthCheckDelay { get; set; }
        public int Port { get; set; }
        public required string LoadBalancerSelector { get; set; }
    }
}
