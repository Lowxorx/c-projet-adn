using System;
using System.Collections.Generic;
using NodeNet.Data;

namespace NodeNet.Worker
{
    public class GenericWorkerFactory : IWorkerFactory
    {
        private static GenericWorkerFactory instance;
        private GenericDictionary workers;

        private GenericWorkerFactory(){
            workers = new GenericDictionary();
        }

        public static GenericWorkerFactory GetInstance()
        {
            if(instance == null)
            {
                instance = new GenericWorkerFactory();
            }
            return instance;
        }

        public void AddWorker<R,T>(String methodName, IWorker<R,T> worker)
        {
            workers.Add(methodName, worker);
        }
        // TODO check if method name exists
        public IWorker<R,T> GetWorker<R, T>(String methodName)
        {
            return workers.GetValue<IWorker<R,T>>(methodName);
        }
    }
}
