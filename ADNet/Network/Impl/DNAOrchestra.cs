using ADNet.Tasks.Impl;
using NodeNet.Data;
using NodeNet.Map_Reduce.Impl;
using NodeNet.Network.Nodes;
using NodeNet.Network.Orch;
using System;
using System.Collections.Generic;


namespace ADNet.Network.Impl
{
    class DNAOrchestra : Orchestrator
    {
        public const String DISPLAY_MESSAGE_METHOD = "DISPLAY_MSG";
        private DNADisplayMsgWorker displayWorker;

        public DNAOrchestra(string name, string address, int port) : base(name, address, port)
        {
            displayWorker = new DNADisplayMsgWorker(ProcessMapReduce, new DisplayMapper(), new DisplayReducer());
            WorkerFactory.AddWorker(DISPLAY_MESSAGE_METHOD, displayWorker);
        }

    }
}
