using System;
using NodeNet.Data;

namespace NodeNet.Tasks
{
    public class GenericTaskExecFactory 
    {
        private static GenericTaskExecFactory instance;
        private static GenericDictionary workers;

        private GenericTaskExecFactory(){}

        public static GenericTaskExecFactory GetInstance()
        {
            if(instance == null)
            {
                instance = new GenericTaskExecFactory();
                workers = new GenericDictionary();
            }
            return instance;
        }

        public void AddWorker<R,T>(String methodName, ITaskExecutor<R,T> worker)
        { 
            workers.Add(methodName, worker);
        }
        // TODO check if method name exists
        public dynamic GetWorker<R, T>(String methodName)
        {
            return workers.GetValue<ITaskExecutor<R, T>>(methodName);
        }
    }
}
