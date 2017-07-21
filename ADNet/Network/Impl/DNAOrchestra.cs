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
        delegate void DisplayDel(string message);
        public DNAOrchestra(string name, string address, int port) : base(name, address, port)
        {
            WorkerFactory.AddWorker(DISPLAY_MESSAGE_METHOD, new DNADisplayMsgWorker(ProcessDisplayMessageFunction));
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

        public override Object ProcessInput(DataInput input)
        {
            dynamic worker = WorkerFactory.GetWorker<Object, Object>(input.Method);
            worker.ProcessResponse("dfsd");
            return null;
        }

        public void ProcessDisplayMessageFunction(String input)
        {
            Console.WriteLine("In process Display from DNAOrchestra");
            ViewModelLocator.VMLOrchStatic.SetMessage("esdsdsds");
        
        }

    }
}
