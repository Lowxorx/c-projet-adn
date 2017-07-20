using System;
using System.Collections.Generic;
using NodeNet.Data;
using System.Collections;

namespace NodeNet.Worker
{
    public class GenericWorkerFactory 
    {
        private static GenericWorkerFactory instance;
        private Dictionary<String, GenericDictionary> _dict;

        private GenericWorkerFactory(){
            _dict = new Dictionary<String, GenericDictionary>();
        }

        public static GenericWorkerFactory GetInstance()
        {
            if(instance == null)
            {
                instance = new GenericWorkerFactory();
            }
            return instance;
        }

        public void AddWorker<R,T>(String methodName, IWorker<R,T> worker, Action method)
        {
            GenericDictionary subDict = new GenericDictionary();
            subDict.Add(worker, method);
            _dict.Add(methodName, subDict);
        }
        // TODO check if method name exists
        public IWorker<R,T> GetWorker<R, T>(String methodName)
        {
            return _dict[methodName].GetWorker<IWorker<R,T>>();
        }

        public Action<Object> GetMethod(String methodName)
        {
            return _dict[methodName].GetMethod<Object>();
        }
    }
}
