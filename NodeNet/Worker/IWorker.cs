using NodeNet.Data;
using System;
using NodeNet.Worker.Impl;

namespace NodeNet.Worker
{
    public interface IWorker<R,T>
    {

        R DoWork(T input);

        void ProcessResponse(R d, Func<R,T> ProcessFunction);

        void CancelWork();
    }
}
