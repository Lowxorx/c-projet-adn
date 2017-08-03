using System;

namespace NodeNet.Tasks
{
    interface ITaskExecFactory
    {
        void AddWorker<R, T, V>(String methodName, ITaskExecutor<R,T, V> worker);
        ITaskExecutor<R,T,V> GetWorker<R, T, V>(String methodName);
    }
}
