using System;
using NodeNet.Data;
using System.Collections.Generic;

namespace NodeNet.Tasks
{
    public class TaskExecFactory 
    {
        private static TaskExecFactory instance;
        private static Dictionary<String,TaskExecutor> workers;

        private TaskExecFactory(){}

        public static TaskExecFactory GetInstance()
        {
            if(instance == null)
            {
                instance = new TaskExecFactory();
                workers = new Dictionary<string, TaskExecutor>();
            }
            return instance;
        }

        public void AddWorker(String methodName, TaskExecutor worker)
        { 
            workers.Add(methodName, worker);
        }
        // TODO check if method name exists
        public TaskExecutor GetWorker(String methodName)
        {
            return (TaskExecutor)workers[methodName].Clone() ;
        }
    }
}
