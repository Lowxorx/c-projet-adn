using System;
using System.Collections.Generic;

namespace NodeNet.Map_Reduce.Impl
{
    public class DisplayMapper : IMapper<List<string>, String>
    {
        public List<string> map(string input)
        {
            List<String> mappedList = new List<string>();
            foreach(char c in input)
            {
                mappedList.Add(c.ToString());
            }
            return mappedList;
        }
    }
}
