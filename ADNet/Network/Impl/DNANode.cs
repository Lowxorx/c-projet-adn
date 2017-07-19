using ADNet.Worker.Impl;
using NodeNet.Network.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace c_projet_adn.Network.Impl
{
    public class DNANode : Node
    {
        public const String DISPLAY_MESSAGE_METHOD = "DISPLAY_MSG";
        public DNANode(String name, String address, int port) : base(name, address, port)
        {
            WorkerFactory.AddWorker(DISPLAY_MESSAGE_METHOD, new DNADisplayMsgWorker());
            Name = name;
            Address = address;
            Port = port;
        }
    }
}
