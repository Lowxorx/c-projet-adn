namespace NodeNet.Tasks
{
    interface ITaskExecFactory
    {
        void AddWorker(string methodName, TaskExecutor worker);
        TaskExecutor GetWorker(string methodName);
    }
}
