using NodeNet.Data;
using System;
using NodeNet.Map_Reduce;

namespace NodeNet.Tasks
{
    public interface ITaskExecutor<R,T,V> : ICloneable
    {

        R NodeWork(T input);

        void OrchWork(DataInput data);

        void ClientWork(DataInput data);

        void CancelWork();


    }
}
