
namespace lb.Model
{
    public class BackendServer
    {
        public int Port { get; set; }

        public string? HostName { get; set; }

        public bool IsHealthy { get; set; }

        public int Weight { get; set; }

        public BackendServer()
        {
        }

        public BackendServer(int port, string? hostName, bool isHealthy, int weight)
        {
            Port = port;
            HostName = hostName;
            IsHealthy = isHealthy;
            Weight = weight;
        }

        public BackendServer(BackendServer other)
        {
            Port = other.Port;
            HostName = other.HostName;
            IsHealthy = other.IsHealthy;
            Weight = other.Weight;
        }

        public override bool Equals(object? obj)
        {
            return obj is BackendServer server &&
                   Port == server.Port &&
                   HostName == server.HostName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Port, HostName);
        }
    }
}
