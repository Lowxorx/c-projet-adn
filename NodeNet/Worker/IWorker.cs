using NodeNet.Data;
using System;


namespace NodeNet.Worker
{
    public interface IWorker<R,T>
    {
        T PrepareData(byte[] data);

        R DoWork(T input);

        void ProcessResponse(R d, Func<R,T> ProcessFunction);

        void CancelWork();

    }
}
