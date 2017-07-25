using ADNet.Tasks.Impl;
using NodeNet.Data;
using NodeNet.Network.Nodes;
using System;

namespace ADNet.Network.Impl
{
    public class DNANode : DefaultNode
    {
        public const String DISPLAY_MESSAGE_METHOD = "DISPLAY_MSG";
        public DNANode(String name, String address, int port) : base(name, address, port)
        {
            WorkerFactory.AddWorker(DISPLAY_MESSAGE_METHOD, new DNADisplayMsgWorker(null,null,null));
            Name = name;
            Address = address;
            Port = port;
        }
    }
}
