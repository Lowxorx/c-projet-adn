using ADNet.Worker.Impl;
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
            displayWorker = new DNADisplayMsgWorker(ProcessDisplayMessageFunction, new DisplayMapper(), new DisplayReducer());
            WorkerFactory.AddWorker(DISPLAY_MESSAGE_METHOD, displayWorker);
        }

        public override Object ProcessInput(DataInput input,Node node)
        {
            base.ProcessInput(input, node);
            if (input.Method != "GET_CPU" && input.Method != "IDENT")
            {
                dynamic worker = WorkerFactory.GetWorker<Object, Object>(input.Method);
                worker.OrchWork(input);
            }
            return null;
        }

        public void ProcessDisplayMessageFunction(DataInput input)
        {
            Console.WriteLine("Process Display Function on Orch");
            if (input.MsgType == MessageType.CALL)
            {
                // MAP
                String message = (String)input.Data;
                List<String> letters = displayWorker.Mapper.map(message);
                String concat = " Concat :";
                foreach (String letter in letters) {
                    concat += letter;
                }

                DataInput res = new DataInput()
                {
                    MsgType = MessageType.RESPONSE,
                    Method = DISPLAY_MESSAGE_METHOD,
                    Data = concat,
                    ClientGUID = input.ClientGUID,
                    NodeGUID = this.NodeGUID,
                };
                SendDataToAllNodes(res);
            }
            else if (input.MsgType == MessageType.RESPONSE)
            {
                String message = (String)input.Data;
                displayWorker.Result = displayWorker.Reducer.reduce(displayWorker.Result, message);
                // TODO check si tous les nodes ont finis
                DataInput res = new DataInput()
                {
                    Method = DISPLAY_MESSAGE_METHOD,
                    Data = displayWorker.Result,
                    ClientGUID = input.ClientGUID,
                    NodeGUID = this.NodeGUID,
                };
                SendData(GetClientFromGUID(input.ClientGUID),res);
            }
        }
    }
}
