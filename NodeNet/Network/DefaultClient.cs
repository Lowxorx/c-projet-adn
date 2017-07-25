using NodeNet.Data;
using NodeNet.GUI.ViewModel;
using NodeNet.Network.Nodes;
using NodeNet.Tasks.Impl;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

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

        /* Multi Client */
        public void RefreshCpuState(DataInput input)
        {
            dynamic worker = WorkerFactory.GetWorker<Object, Object>(input.Method);
            Console.WriteLine("process cpu state");
            ViewModelLocator.VMLMonitorUcStatic.RefreshNodesInfo(input);
        }

        public void StartMonitorNodes()
        {
            Console.WriteLine("Start monitor node");
            DataInput input = new DataInput()
            {
                Method = GET_CPU_METHOD,
                Data = null,
                ClientGUID = NodeGUID,
                NodeGUID = NodeGUID,
                MsgType = MessageType.CALL
            };
            SendData(Orch, input);
        }

        public void AddNodeToList(List<List<string>> monitoringValues)
        {
            foreach (List<string> nodeinfo in monitoringValues)
            {
                DefaultNode n = new DefaultNode(nodeinfo[0], nodeinfo[1], Convert.ToInt32(nodeinfo[2]));
                Application.Current.Dispatcher.Invoke(new Action(() => ViewModelLocator.VMLMonitorUcStatic.NodeList.Add(n)));      
            }
        }

        public void ProcessIdent(DataInput input)
        {
            if (input.Data != null)
            {
                AddNodeToList((List<List<string>>)input.Data);
            }
            else
            {
                input.Data = new Tuple<bool, string>(true, NodeGUID);
                input.ClientGUID = NodeGUID;
                input.NodeGUID = NodeGUID;
                SendData(Orch, input);
                Thread.Sleep(10000);
                StartMonitorNodes();
            }
        }
    }
}
