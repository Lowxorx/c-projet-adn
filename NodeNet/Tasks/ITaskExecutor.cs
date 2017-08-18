using NodeNet.Data;
using System;
using NodeNet.Map_Reduce;

namespace NodeNet.Tasks
{
    public interface ITaskExecutor : ICloneable
    {
        Object DoWork(DataInput data);
    }
}
