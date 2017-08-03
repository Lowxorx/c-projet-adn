using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet.Map_Reduce
{
    public interface IReducer<V, F>
    {
        F reduce(V concat,V input);
    }
}
