using System;
using NodeNet.Data;
using NodeNet.Network.Nodes;
using NodeNet.Map_Reduce;

namespace NodeNet.Tasks.Impl
{
    public class IdentificationTask : ITaskExecutor<Tuple<String, int>, String>
    {
        public IMapper<Tuple<String, int>, String> Mapper { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IReducer<Tuple<String, int>, String> Reducer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Action<DataInput> ProcessAction;

        public IdentificationTask(Action<DataInput> action)
        {
            ProcessAction = action;
        }

        public void CancelWork()
        {
            throw new NotImplementedException();
        }

        public String CastInputData(object data)
        {
            return (String)data;
        }

        public Tuple<String, int> NodeWork(String input)
        {
            return null;
        }

        public void OrchWork(DataInput input)
        {
            ProcessAction(input);
        }

        public void ClientWork(DataInput input)
        {
            ProcessAction(input);
        }

        public Tuple<String, int> CastOutputData(object data)
        {
            return (Tuple<String, int>)data;
        }
    }
}
