using lb.Model;

namespace lb
{
    public interface IHealthCheckObserver
    {
        void Update(BackendServer server);
    }
}