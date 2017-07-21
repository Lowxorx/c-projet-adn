using NodeNet.Data;
using System;
using NodeNet.Worker.Impl;

namespace NodeNet.Worker
{
    public interface IWorker<R,T>
    {
        IMapper<R, T> Mapper { get; set; }
        IReducer<R, T> Reducer { get; set; }

        R DoWork(T input);

        void ProcessResponse(R input);

        void CancelWork();

        T CastInputData(Object data);

        R CastOutputData(Object data);

        
    }
}
