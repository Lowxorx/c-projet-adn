using System;
using NodeNet.Data;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace NodeNet.Tasks
{
    public class TaskExecFactory 
    {
        private static TaskExecFactory instance;
        private static ConcurrentDictionary<String,TaskExecutor> workers;

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

        public void AddWorker(String methodName, TaskExecutor worker)
        { 
            workers.TryAdd(methodName, worker);
        }
        // TODO check if method name exists
        public TaskExecutor GetWorker(String methodName)
        {
            return (TaskExecutor)workers[methodName].Clone() ;
        }
    }
}
