using System;

namespace NodeNet.Tasks
{
    interface ITaskExecFactory
    {
        void AddWorker(String methodName, TaskExecutor worker);
        TaskExecutor GetWorker(String methodName);
    }
}
