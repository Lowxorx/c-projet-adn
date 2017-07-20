using System;

namespace NodeNet.Worker
{
    interface IWorkerFactory
    {
        void AddWorker<R, T>(String methodName, IWorker<R,T> worker);
        IWorker<R,T> GetWorker<R, T>(String methodName);
    }
}
