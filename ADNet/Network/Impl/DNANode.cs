using ADNet.Worker.Impl;
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
        public override Object ProcessInput(DataInput input,Node node)
        {
            base.ProcessInput(input,node);
            Console.WriteLine("ProcessInput for " + input.Method );
            dynamic worker = WorkerFactory.GetWorker<Object, Object>(input.Method);
            Object result = worker.NodeWork(worker.CastInputData(input.Data));
            return result;
        }
    }
}
