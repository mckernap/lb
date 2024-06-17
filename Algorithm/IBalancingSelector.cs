using lb.Model;

namespace lb.Algorithm
{
    public interface IBalancingSelector
    {
        public BackendServer? Next();
    }
}