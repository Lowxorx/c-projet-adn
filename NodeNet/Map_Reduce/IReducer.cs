using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet.Map_Reduce
{
    public interface IReducer<R,T>
    {
        R reduce(R concat,T input);
    }
}
