using ADNet.GUI.ViewModel;
using ADNet.Worker.Impl;
using NodeNet.Data;
using NodeNet.Network.Nodes;
using NodeNet.Network.Orch;
using NodeNet.Worker;
using NodeNet.Worker.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace ADNet.Network.Impl
{
    class DNAOrchestra : Orchestrator
    {
        public const String DISPLAY_MESSAGE_METHOD = "DISPLAY_MSG";

        public DNAOrchestra(string name, string address, int port) : base(name, address, port)
        {
            WorkerFactory.AddWorker(DISPLAY_MESSAGE_METHOD, new DNADisplayMsgWorker<String>(), ProcessDisplayMessageFunction);
        }

        public void SendMessage(String msg)
        {
            DataInput input = new DataInput()
            {
                Method = DISPLAY_MESSAGE_METHOD,
                Data = DataFormater.Serialize(msg),
                MsgType = MessageType.CALL
            };
            SendDataToAllNodes(input);
        }
        


       

        public override void ProcessInput(DataInput input)
        {
            Action<Object> act = WorkerFactory.GetMethod(input.Method);
            WorkerFactory.GetWorker<Object, Object>(input.Method).ProcessResponse(input.Data, act);
        }

        public void ProcessDisplayMessageFunction(Object input)
        {
            Console.WriteLine("In process Display from DNAOrchestra");
            ViewModelLocator.VMLOrchStatic.SetMessage("esdsdsds");
        }
    }
}
