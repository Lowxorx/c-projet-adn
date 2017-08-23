using System;
using NodeNet.Data;
using NodeNet.Map_Reduce;
using NodeNet.Network.Nodes;

namespace NodeNet.Tasks
{
    public class TaskExecutor : ITaskExecutor
    {
        protected Node Executor { get; set; }

        protected int NbWorkers = 0;
        protected int NbWorkersDone = 0;

        public IMapper Mapper { get; set; }
        public IReducer Reducer { get; set; }

        public Func<DataInput,object> ProcessAction;

        public TaskExecutor(Node node, Func<DataInput, object> function,IMapper mapper,IReducer reducer)
        {
            Executor = node;
            Mapper = mapper;
            Reducer = reducer;
            ProcessAction = function;
        }

        public object DoWork(DataInput input)
        {
            return ProcessAction(input);
        }

        public object Clone()
        {
            IMapper newMapper = null;
            IReducer newReducer = null;
            if (Mapper != null && Reducer != null)
            {
                newMapper = (IMapper)Mapper.Clone();
                newReducer = (IReducer)Reducer.Clone();
            }
            return new TaskExecutor(Executor,ProcessAction, newMapper, newReducer);
        }
    }
}
