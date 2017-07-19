using System;
using System.Collections.Generic;

namespace NodeNet.Worker
{
    public class GenericWorkerFactory : IWorkerFactory
    {
        private static GenericWorkerFactory instance;
        private Dictionary<String, IWorker> workers;

        private GenericWorkerFactory(){
            workers = new Dictionary<string, IWorker>();
        }

        public static GenericWorkerFactory getInstance()
        {
            if(instance == null)
            {
                instance = new GenericWorkerFactory();
            }
            return instance;
        }

        public void AddWorker(String methodName, IWorker worker)
        {
            workers.Add(methodName, worker);
        }
        // TODO check if method name exists
        public IWorker GetWorker(String methodName)
        {
            return workers[methodName];
        }
    }
}
