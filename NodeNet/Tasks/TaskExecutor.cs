using System;
using NodeNet.Data;
using NodeNet.Map_Reduce;
using System.ComponentModel;
using NodeNet.Network.Nodes;

namespace NodeNet.Tasks
{
    public class TaskExecutor : ITaskExecutor
    {
        protected Node executor { get; set; }

        protected int NbWorkers = 0;
        protected int NbWorkersDone = 0;

        public IMapper Mapper { get; set; }
        public IReducer Reducer { get; set; }

        public Func<DataInput,Object> ProcessAction;

        public TaskExecutor(Node node, Func<DataInput, Object> function,IMapper mapper,IReducer reducer)
        {
            executor = node;
            Mapper = mapper;
            Reducer = reducer;
            ProcessAction = function;
        }

        public Object DoWork(DataInput input)
        {
            return ProcessAction(input);
        }

        public object Clone()
        {
            return new TaskExecutor(executor,ProcessAction,Mapper,Reducer);
        }
    }
}
