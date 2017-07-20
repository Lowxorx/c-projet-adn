using NodeNet.Data;
using System;
using NodeNet.Worker.Impl;

namespace NodeNet.Worker
{
    public interface IWorker<R,T>
    {
        IMapper<R, T> mapper { get; set; }
        IReducer<R, T> reducer { get; set; }

        R DoWork(T input);

        void ProcessResponse(Object data, Action<Object> ProcessFunction);

        void CancelWork();
    }
}
