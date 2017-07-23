using System;
using NodeNet.Network;
using NodeNet.Map_Reduce;
using NodeNet.Worker;
using NodeNet.Worker.Impl;
using NodeNet.Data;
using System.Collections.Generic;

namespace ADNet.Worker.Impl
{
    public class DNADisplayMsgWorker : IWorker<String, String>
    {
        public IMapper<String, String> Mapper { get ; set; }
        public IReducer<String, String> Reducer { get ; set; }

        public String Result;

        public Action<DataInput> ProcessFunction;

        public DNADisplayMsgWorker(Action<DataInput> function, IMapper<String, String> mapper, IReducer<String, String> reducer)
        {
            Mapper = mapper;
            Reducer = reducer;
            ProcessFunction = function;
        }

        public void CancelWork()
        {
            throw new NotImplementedException();
        }

        public String NodeWork(String input)
        {
            Console.WriteLine("Node process display and return message");
            return input + "Node work";
        }

        public void OrchWork(DataInput input)
        {
            ProcessFunction(input);
        }

        public string CastInputData(object data)
        {
            return (String)data;
        }

        public void ClientWork(DataInput input)
        {
            ProcessFunction(input);
        }
    }
}
