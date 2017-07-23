using NodeNet.Network.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNet.Data;
using System.Net.Sockets;
using NodeNet.GUI.ViewModel;
using NodeNet.Tasks.Impl;
using System.ComponentModel;

namespace NodeNet.Network
{
    public abstract class DefaultClient : Node
    {
        public const String GET_CPU_METHOD = "GET_CPU";
        public const String IDENT_METHOD = "IDENT";

        public DefaultClient(String name, String adress, int port) : base(name,adress,port) {
            WorkerFactory.AddWorker(GET_CPU_METHOD, new CPUStateTask(RefreshCpuState));
            WorkerFactory.AddWorker(IDENT_METHOD, new IdentificationTask(ProcessIdent));
        }

        public DefaultClient(string name, string adress, int port, Socket sock) : base(name,adress,port, sock){}

        public override object ProcessInput(DataInput input,Node node)
        {
            Console.WriteLine("ProcessInput in Client");
            dynamic worker = WorkerFactory.GetWorker<Object, Object>(input.Method);
            worker.ClientWork(input);
            return null;
        }

        public abstract void RefreshCpuState(DataInput data);

        public void ProcessIdent(DataInput data)
        {
            data.Data =  new Tuple<bool, string>(true, NodeGUID);
            data.ClientGUID = NodeGUID;
            data.NodeGUID = NodeGUID;
            SendData(Orch, data);
        }
    }
}
