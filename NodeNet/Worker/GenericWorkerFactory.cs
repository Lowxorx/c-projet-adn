using System;
using NodeNet.Data;

namespace NodeNet.Worker
{
    public class GenericWorkerFactory 
    {
        private static GenericWorkerFactory instance;
        private static GenericDictionary workers;

        private GenericWorkerFactory(){}

        public static GenericWorkerFactory GetInstance()
        {
            if(instance == null)
            {
                instance = new GenericWorkerFactory();
                workers = new GenericDictionary();
            }
            return instance;
        }

        public void AddWorker<R,T>(String methodName, IWorker<R,T> worker)
        { 
            workers.Add(methodName, worker);
        }
        // TODO check if method name exists
        public dynamic GetWorker<R, T>(String methodName)
        {
            return workers.GetValue<IWorker<R, T>>(methodName);
        }
    }
}
