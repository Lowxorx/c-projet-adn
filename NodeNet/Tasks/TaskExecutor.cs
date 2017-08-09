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
            return null;
        }

        #region EventHandlers
        protected void Backgroundworker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.NbWorkers++;
            DataInput input = new DataInput()
            {
                Method = "TASK_STATE",
                NodeGUID = executor.NodeGUID,

                
            };
        }

        public void Backgroundworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        public void Backgroundworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        public void Backgroundworker_DoWork(DataInput data, Node sender)
        {
           

            //DataInput input = new DataInput()
            //{
            //    Method = "TASK_STATE",
            //    NodeGUID = executor.NodeGUID,


            //};
        }
        #endregion
    }
}
