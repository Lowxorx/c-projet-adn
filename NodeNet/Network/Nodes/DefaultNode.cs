using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NodeNet.Data;

namespace NodeNet.Network.Nodes
{
    public class DefaultNode : Node
    {
        public DefaultNode(String name, String adress, int port) : base(name,adress,port)
        {
        }

        public DefaultNode(string name, string adress, int port, Socket sock) : base(name,adress,port, sock)
        {
        }

        public override object ProcessInput(DataInput input)
        {
            throw new NotImplementedException();
        }
    }
}
