using System.Collections.Concurrent;

namespace NodeNet.Tasks
{
    public class TaskExecFactory 
    {
        private static TaskExecFactory instance;
        private static ConcurrentDictionary<string,TaskExecutor> workers;

        private TaskExecFactory(){}

        public static TaskExecFactory GetInstance()
        {
            if(instance == null)
            {
                instance = new TaskExecFactory();
                workers = new ConcurrentDictionary<string, TaskExecutor>();
            }
            return instance;
        }

        public void AddWorker(string methodName, TaskExecutor worker)
        { 
            workers.TryAdd(methodName, worker);
        }
        // TODO check if method name exists
        public TaskExecutor GetWorker(string methodName)
        {
            return (TaskExecutor)workers[methodName].Clone() ;
        }
    }
}
