
using ADNet.GUI.ViewModel;
using NodeNet.Network;
using NodeNet.Network.Nodes;
using System;
using System.Net.Sockets;
using NodeNet.Data;
using NodeNet.Tasks;

namespace c_projet_adn.Network.Impl
{
    public class DNAClient : DefaultClient
    {
        public const String DISPLAY_MESSAGE_METHOD = "DISPLAY_MSG";
        public const String IDENT_METHOD = "IDENT";


        public DNAClient(String name, String adress, int port) : base(name,adress,port)
        {
            WorkerFactory.AddWorker(DISPLAY_MESSAGE_METHOD, new TaskExecutor(this,ProcessDisplayMessageFunction,null,null));
            //WorkerFactory.AddWorker(IDENT_METHOD, new IdentitifierWorker(ProcessIdent));
        }

        public DNAClient(string name, string adress, int port, Socket sock) : base(name,adress,port, sock){}

        //public override object ProcessInput(DataInput input,Node node)
        //{
        //    Console.WriteLine("In ProcessInput DNAClient");
        //    dynamic worker = WorkerFactory.GetWorker<Object, Object>(input.Method);
        //    worker.ClientWork(input);
        //    return null;
        //}

        public void SendMessage(String msg)
        {
            Console.WriteLine("Send Msssage from Client : " + msg);
            DataInput input = new DataInput()
            {
                Method = DISPLAY_MESSAGE_METHOD,
                Data = msg,
                ClientGUID = NodeGUID,
                NodeGUID = NodeGUID,
                MsgType = MessageType.CALL
             
            };
            SendData(Orch, input);
        }

        public Object ProcessDisplayMessageFunction(DataInput input)
        {
            Console.WriteLine("Client Process Display Response From Orchestrator Msg : " + input.Data);
            ViewModelLocator.VMLCliStatic.SetMessage((String)input.Data);
            return null;
        }
    }
}