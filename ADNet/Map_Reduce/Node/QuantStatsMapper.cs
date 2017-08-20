using NodeNet.Map_Reduce;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ADNet.Map_Reduce.Node
{
    public class QuantStatsMapper : IMapper
    {
        public int nbrchar { get; set; }

        public Object map(Object input)
        {
            nbrchar = 100000;
            List<char[]> result = new List<char[]>();
            char[] data = (char[])input;
            int currentIndex = 0;
            while(currentIndex < data.Length)
            {
                char[] chunk = new char[nbrchar];
                for (int i = currentIndex; i + currentIndex < data.Length && i < nbrchar; i++)
                {
                    chunk[i] = data[currentIndex];
                    currentIndex++;
                }
                result.Add(chunk);
            }
            return result;
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
