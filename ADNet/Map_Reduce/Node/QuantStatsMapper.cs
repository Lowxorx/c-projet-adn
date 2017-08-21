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
            nbrchar = 1000;
            List<char[]> result = new List<char[]>();
            char[] data = (char[])input;
            Console.WriteLine("In mapping data[] size :" + data.Length);
            int currentIndex = 0;
            Console.WriteLine(" data size : " + data.Length);
            while (currentIndex < data.Length)
            {
                int resultSize = data.Length - currentIndex > nbrchar ? nbrchar : data.Length - currentIndex;
                char[] chunk = new char[resultSize];
                for (int i = currentIndex, j = 0; i < data.Length && j < nbrchar; i++, j++)
                {
                    chunk[j] = data[i];
                    Console.WriteLine("char :" + data[i] + " add to chunk " + result.Count + "current index : " + currentIndex);
                    currentIndex++;
                }
                Console.WriteLine("Add chunk in BG ") ;
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
