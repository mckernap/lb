using lb.Model;

namespace lb.Provider
{
    public interface IHealthCheckSubject
    {
        void Subscribe(IHealthCheckObserver observer);
        void Unsubscribe(IHealthCheckObserver observer);
        void Notify(BackendServer server);
    }
}
