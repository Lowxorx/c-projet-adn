using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NodeNet.Data;
using NodeNet.Worker;
using NodeNet.Worker.Impl;
using NodeNet.Network.Orch;

namespace NodeNet.Network.Nodes
{
    public class DefaultNode : Node
    {
        public DefaultNode(String name, String adress, int port) : base(name,adress,port)
        {
            WorkerFactory = GenericWorkerFactory.GetInstance();
            try
            {
                WorkerFactory.AddWorker("IDENT", new IdentitifierWorker(null));
                WorkerFactory.AddWorker("GET_CPU", new CPUStateWorker(null));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public DefaultNode(string name, string adress, int port, Socket sock) : base(name,adress,port, sock)
        {
        }

        public override object ProcessInput(DataInput input,Node node)
        {
            Console.WriteLine("Process input in defualt node");
            if (input.Method == "IDENT")
            {
                DataInput resp = new DataInput()
                {
                    ClientGUID = null,
                    NodeGUID = NodeGUID,
                    MsgType = MessageType.RESPONSE
                };
                return resp;
            }
            return null;
        }
    }
}
