using NodeNet.Data;
using System;

namespace NodeNet.Tasks
{
    public interface ITaskExecutor : ICloneable
    {
        object DoWork(DataInput data);
    }
}
