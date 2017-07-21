using ADNet.Worker.Impl;
using NodeNet.Data;
using NodeNet.Network.Nodes;
using NodeNet.Worker;
using NodeNet.Worker.Impl;
using System;
using System.Net.Sockets;

namespace ADNet.Network.Impl
{
    public class DNANode : Node
    {
        public const String DISPLAY_MESSAGE_METHOD = "DISPLAY_MSG";
        public DNANode(String name, String address, int port) : base(name, address, port)
        {
            WorkerFactory.AddWorker(DISPLAY_MESSAGE_METHOD, new DNADisplayMsgWorker(null));
            Name = name;
            Address = address;
            Port = port;
        }
        public override Object ProcessInput(DataInput input)
        {
            dynamic worker = WorkerFactory.GetWorker<Object, Object>(input.Method);
            Object result = worker.DoWork("qssqd");
            return result;
        }
    }
}
