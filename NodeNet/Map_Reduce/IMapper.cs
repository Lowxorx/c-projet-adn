using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet.Map_Reduce
{
    public interface IMapper<R,T>
    {
        R map(T input);
    }
}
