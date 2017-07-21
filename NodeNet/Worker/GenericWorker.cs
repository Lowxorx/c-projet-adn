using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNet.Data;

namespace NodeNet.Worker
{
    public abstract class GenericWorker<R, T> : IWorker<R, T>
    {
        public abstract IMapper<R, T> mapper { get; set; }
        public abstract IReducer<R, T> reducer { get; set; }

        public abstract T PrepareData(object data);

        public abstract void CancelWork();
        public abstract R DoWork(T input);
        public abstract void ProcessResponse(T input);
    }
}
