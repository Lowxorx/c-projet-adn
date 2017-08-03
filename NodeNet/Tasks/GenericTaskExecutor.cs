using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNet.Data;
using NodeNet.Map_Reduce;
using System.ComponentModel;
using NodeNet.Misc;

namespace NodeNet.Tasks
{
    public abstract class GenericTaskExecutor<R, T, V> : ITaskExecutor<R, T, V>
    {
        protected int NbWorkers = 0;
        protected int NbWorkersDone = 0;
        public abstract IMapper<R, T> Mapper { get; set; }
        public abstract IReducer<V, V> Reducer { get; set; }

        public abstract void CancelWork();
        public abstract void ClientWork(DataInput data);
        public abstract R NodeWork(T input);
        public abstract void OrchWork(DataInput data);


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
        }

        protected void Backgroundworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        protected void Backgroundworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
        #endregion
    }
}
