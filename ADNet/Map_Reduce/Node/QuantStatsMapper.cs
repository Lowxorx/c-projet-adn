using NodeNet.Map_Reduce;
using System;
using System.Collections.Generic;

namespace ADNet.Map_Reduce.Node
{
    public class QuantStatsMapper : IMapper
    {
        public Object map(Object input)
        {
            List<string> list = new List<string>();

            foreach(String s in ((string)input).Split('\t'))
            {
                list.Add(s);
            }
            return list;
        }

        public bool mapIsEnd()
        {
            throw new NotImplementedException();
        }

        IMapper IMapper.reset()
        {
            return this;
        }

        public object Clone()
        {
            return new QuantStatsMapper();
        }
    }   
}
