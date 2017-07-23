using System;

namespace NodeNet.Tasks
{
    interface ITaskExecFactory
    {
        void AddWorker<R, T>(String methodName, ITaskExecutor<R,T> worker);
        ITaskExecutor<R,T> GetWorker<R, T>(String methodName);
    }
}
