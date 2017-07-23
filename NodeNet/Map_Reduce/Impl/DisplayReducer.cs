using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet.Map_Reduce.Impl
{
    public class DisplayReducer : IReducer<String, String>
    {
        public string reduce(string concat, string input)
        {
            return concat + " " + input;
        }
    }
}
