using NodeNet.Map_Reduce;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ADNet.Map_Reduce.Node
{
    public class QuantStatsMapper : IMapper
    {
        public Object map(Object input,int nbMap)
        {

            List<char[]> result = new List<char[]>();
            char[] data = (char[])input;
            int totalNbChar = data.Length;
            int nbCharByChunk = totalNbChar / nbMap;
            int rest = totalNbChar % nbMap;
            int currentChar = 0;

            for(int i = 0; i < nbMap; i++)
            {
                if (i == 0)
                {
                    if(rest < nbCharByChunk / 2)
                    {
                        rest += nbCharByChunk;
                        i++;
                    }
                    char[] chunk = new char[rest];
                    for(int j = 0; j < rest; j++)
                    {
                        chunk[j] = data[currentChar];
                        currentChar++;
                    }
                    result.Add(chunk);
                }
                else
                {
                    char[] chunk = new char[nbCharByChunk];
                    for (int j = 0; j < nbCharByChunk; j++)
                    {
                        chunk[j] = data[currentChar];
                        currentChar++;
                    }
                    result.Add(chunk);
                }
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
