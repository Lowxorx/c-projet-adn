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
        public abstract IMapper<R, T> Mapper { get; set; }
        public abstract IReducer<R, T> Reducer { get; set; }

        public abstract R CastData(object data);

        public abstract void CancelWork();
        public abstract R DoWork(T input);
        public abstract void ProcessResponse(R input);
    }
}
