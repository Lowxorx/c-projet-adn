
using ADNet.Worker.Impl;
using NodeNet.Network.Data;
using NodeNet.Network.Orch;
using System;

namespace ADNet.Network.Impl
{
    class DNAOrchestra : Orchestrator
    {
        public const String DISPLAY_MESSAGE_METHOD = "DISPLAY_MSG";

        public DNAOrchestra(string name, string address, int port) : base(name, address, port)
        {
            WorkerFactory.AddWorker(DISPLAY_MESSAGE_METHOD, new DNADisplayMsgWorker());
        }

        public void SendMessage(String msg)
        {
            DataInput input = new DataInput()
            {
                Method = DISPLAY_MESSAGE_METHOD,
                Data = DataFormater.Serialize(msg),
                msgType = MessageType.CALL
            };
            SendDataToAllNodes(input);
        }

        public new void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
