using NodeNet.Data;
using System;
using NodeNet.Map_Reduce;

namespace NodeNet.Tasks
{
    public interface ITaskExecutor<R,T>
    {
        IMapper<R, T> Mapper { get; set; }
        IReducer<R, T> Reducer { get; set; }

        R NodeWork(T input);

        void OrchWork(DataInput data);

        void ClientWork(DataInput data);

        void CancelWork();

        T CastInputData(Object data);

    }
}
