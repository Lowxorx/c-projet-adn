using NodeNet.Data;
using NodeNet.GUI.ViewModel;
using NodeNet.Network.Nodes;
using NodeNet.Network.States;
using NodeNet.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Windows;

namespace NodeNet.Network
{
    public abstract class DefaultClient : Node
    {
        protected DefaultClient(string name, string adress, int port) : base(name,adress,port)
        {
            WorkerFactory.AddWorker(GetCpuMethod, new TaskExecutor(this,RefreshCpuState,null,null));
            WorkerFactory.AddWorker(IdentMethod, new TaskExecutor(this,ProcessIdent,null,null));
            WorkerFactory.AddWorker(TaskStatusMethod, new TaskExecutor(this, RefreshTaskState, null, null));
        }

        protected DefaultClient(string name, string adress, int port, Socket sock) : base(name,adress,port, sock){}

        public override void ProcessInput(DataInput input,Node node)
        {
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
            if (MethodIsNotInfra(input.Method) && input.MsgType == MessageType.Response)
            {
                ViewModelLocator.VmlMonitorUcStatic.RefreshStateFromTaskResult(input);
            }
            object res = executor.DoWork(input);
            if (res != null)
            {
                DataInput resp = new DataInput()
                {
                    ClientGuid = input.ClientGuid,
                    NodeGuid = NodeGuid,
                    TaskId = input.TaskId,
                    Method = input.Method,
                    Data = res,
                    MsgType = MessageType.Response
                };
                SendData(node, resp);
            }
        }

        /* Multi Client */
        public object RefreshCpuState(DataInput input)
        {
            ViewModelLocator.VmlMonitorUcStatic.RefreshNodesInfo(input);
            return null;
        }

        public void StartMonitorNodes()
        {
            Console.WriteLine(@"Launch Cli");
            Logger.Write("Start monitoring node...", false);
            DataInput input = new DataInput()
            {
                Method = GetCpuMethod,
                Data = null,
                ClientGuid = NodeGuid,
                NodeGuid = NodeGuid,
                MsgType = MessageType.Call
            };
            SendData(Orch, input);
        }

        public void AddNodeToList(List<List<string>> monitoringValues)
        {
            foreach (List<string> nodeinfo in monitoringValues)
            {
                DefaultNode n = new DefaultNode(nodeinfo[0], nodeinfo[1], Convert.ToInt32(nodeinfo[2]));
                Application.Current.Dispatcher.Invoke(() => ViewModelLocator.VmlMonitorUcStatic.NodeList.Add(n));      
            }
        }

        public object ProcessIdent(DataInput input)
        {
            if (input.MsgType == MessageType.NodeIdent)
            {
                AddNodeToList((List<List<string>>)input.Data);
            }
            else if(input.MsgType == MessageType.Ident)
            {
                Tuple<string, int> orchIDentifiers = (Tuple < string, int>) input.Data;
                Name = Name + orchIDentifiers.Item1;
                Port = orchIDentifiers.Item2;
                GenGuid();
                input.ClientGuid = NodeGuid;
                return input;
            }
            return null;
        }

        private object RefreshTaskState(DataInput input)
        {
           Tuple<NodeState,object> state = (Tuple<NodeState,object>)input.Data;
            switch (state.Item1)
            {
                case NodeState.JobStart:
                    ViewModelLocator.VmlMonitorUcStatic.CreateTask((Task)state.Item2);
                    break;
                case NodeState.NodeIsWorking:
                ViewModelLocator.VmlMonitorUcStatic.NodeIsWorkingOnTask((string)state.Item2,input.TaskId);
                break;
                case NodeState.Work:
                    ViewModelLocator.VmlMonitorUcStatic.RefreshNodesState(input.NodeGuid, NodeState.Finish);
                    ViewModelLocator.VmlMonitorUcStatic.RefreshTaskState(input.TaskId, (double)state.Item2);
                    break;
                case NodeState.Error:
                    ViewModelLocator.VmlMonitorUcStatic.NodeIsFailed(input.NodeGuid);
                    ViewModelLocator.VmlMonitorUcStatic.CancelTask(input.Data as List<Task>);
                    break;
            }
            return null;
        }

        public override void RemoveDeadNode(Node node)
        {
            MessageBox.Show("Erreur sur l'orchestrateur. Fermeture de l'application...");
            Process.GetCurrentProcess().CloseMainWindow();
        }
    }
}
