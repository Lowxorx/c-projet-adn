using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNet.Data;
using NodeNet.Map_Reduce;
using System.ComponentModel;
using NodeNet.Misc;
using NodeNet.Network.Nodes;

namespace NodeNet.Tasks
{
    public abstract class GenericTaskExecutor<R, T, V> : ITaskExecutor<R, T, V>
    {
        protected Node executor { get; set; }

        protected int NbWorkers = 0;
        protected int NbWorkersDone = 0;
        public abstract IMapper<R, T> Mapper { get; set; }
        public abstract IReducer<V, V> Reducer { get; set; }

        public abstract void CancelWork();
        public abstract void ClientWork(DataInput data);
        public abstract R NodeWork(T input);
        public abstract void OrchWork(DataInput data);

        public GenericTaskExecutor(Node node){ executor = node; }

        protected T CastInputData(Object data)
        {
            return (T)data;
        }

        protected R CastOutputData(Object data)
        {
            return (R) data;
        }

        public abstract object Clone();

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

        protected void Backgroundworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        protected void Backgroundworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        protected void Backgroundworker_DoWork(DataInput data, sender)
        {
            dynamic worker = WorkerFactory.GetWorker<Object, Object, Object>(data.Method);
            this.NbWorkers++;

            //DataInput input = new DataInput()
            //{
            //    Method = "TASK_STATE",
            //    NodeGUID = executor.NodeGUID,


            //};
        }
        #endregion
    }
}
