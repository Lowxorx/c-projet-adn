using NodeNet.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet
{
    [Serializable]
    public class DataInput
    {
        public string msg { get; set; }
        public State status { get; set; }
        public byte[] data { get; set; }
        public string cpu { get; set; }

        public enum request { status, msg, data }
        public request query {get;set;}
        public DataInput( request req)
        {
            query = req;
        }
    }
}
