using System;

namespace NodeNet.Worker
{
    interface IWorkerFactory
    {
        void AddWorker(String methodName, IWorker worker);
        IWorker GetWorker(String methodName);
    }
}
