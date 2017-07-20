using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet.Data
{
    public interface IMapper<R,T>
    {
        R map(T input);
    }
}
