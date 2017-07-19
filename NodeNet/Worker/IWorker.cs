using NodeNet.Network.Data;

namespace NodeNet.Worker
{
    public interface IWorker
    {
        DataInput DoWork(DataInput input);

        void CancelWork();

    }
}
