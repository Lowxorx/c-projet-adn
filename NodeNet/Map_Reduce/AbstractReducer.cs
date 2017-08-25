using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NodeNet.Map_Reduce
{
    public abstract class AbstractReducer : IReducer
    {
        public abstract object Clone();
        public abstract object Reduce(ConcurrentBag<object> input);

        public List<Tuple<int, object>> SortByTaskId(ConcurrentBag<object> input)
        {
            List<Tuple<int, object>> list = new List<Tuple<int, object>>();
            foreach (Tuple<int, object> item in input)
            {
                list.Add(item);
            }
            list.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            return list;
        }
    }
}
