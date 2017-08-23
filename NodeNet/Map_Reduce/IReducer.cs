using System;
using System.Collections.Concurrent;

namespace NodeNet.Map_Reduce
{
    public interface IReducer : ICloneable
    {
        object Reduce(ConcurrentBag<object> input);
        new object Clone();
    }
}
