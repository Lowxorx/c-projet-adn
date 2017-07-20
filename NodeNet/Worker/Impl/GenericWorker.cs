using NodeNet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet.Worker.Impl
{
    public abstract class GenericWorker<R, T> : IWorker<R, T>
    {
        public IMapper<R,T> mapper { get; set; }
        public IReducer<R, T> reducer { get; set; }
        public abstract void CancelWork();

        public abstract R DoWork(T input);

        public T PrepareData(byte[] data)
        {
            return DataFormater.Deserialize<T>(data);
        }

        public abstract void ProcessResponse(R d, Func<R, T> ProcessFunction);
    }
}
