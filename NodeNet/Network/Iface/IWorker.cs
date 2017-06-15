using System.Threading.Tasks;

namespace NodeNet.Network.Iface
{
    public interface IWorker<R,T>
    {
        R doWork(T input);

        State getState();

        void cancelWork();

    }
}
