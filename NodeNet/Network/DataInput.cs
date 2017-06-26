using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet
{
    [Serializable]
    public class DataInput<T,R>
    {
        public T input { get; set; }
        public DataInput(T input)
        {
            this.input = input;
        }
    }
}
