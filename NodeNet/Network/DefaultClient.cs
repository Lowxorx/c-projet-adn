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

        public DefaultClient(String name, String adress, int port) : base(name,adress,port) {
            WorkerFactory.AddWorker(GET_CPU_METHOD, new TaskExecutor(this,RefreshCpuState,null,null));
            WorkerFactory.AddWorker(IDENT_METHOD, new TaskExecutor(this,ProcessIdent,null,null));
            WorkerFactory.AddWorker(TASK_STATUS_METHOD, new TaskExecutor(this, RefreshTaskState, null, null));
        }

        public DefaultClient(string name, string adress, int port, Socket sock) : base(name,adress,port, sock){}

        public override void ProcessInput(DataInput input,Node node)
        {
            if (input.Method != GET_CPU_METHOD)
            {
                Console.WriteLine("Process input for : " + input.Method + " at : " + DateTime.Now.ToLongTimeString());
            }
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
            Object res = executor.DoWork(input);
            if (res != null)
            {
                DataInput resp = new DataInput()
                {
                    ClientGUID = input.ClientGUID,
                    NodeGUID = NodeGUID,
                    TaskId = input.TaskId,
                    Method = input.Method,
                    Data = res,
                    MsgType = MessageType.RESPONSE
                };
                SendData(node, resp);
            }
        }

        /* Multi Client */
        public Object RefreshCpuState(DataInput input)
        {
            ViewModelLocator.VMLMonitorUcStatic.RefreshNodesInfo(input);
            return null;
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

        public Object ProcessIdent(DataInput input)
        {
            if (input.MsgType == MessageType.NODE_IDENT)
            {
                AddNodeToList((List<List<string>>)input.Data);
            }
            else if(input.MsgType == MessageType.IDENT)
            {
                Tuple<String, int> orchIDentifiers = (Tuple < String, int>) input.Data;
                Name = Name + orchIDentifiers.Item1;
                Port = orchIDentifiers.Item2;
                genGUID();
                input.ClientGUID = NodeGUID;
                return input;
            }
            return null;
        }

        private object RefreshTaskState(DataInput input)
        {
            List<Task> listTask = (List < Task > )input.Data;

            foreach(Task task in listTask)
            {
                switch (task.State)
                {
                    case NodeState.WORK:
                        ViewModelLocator.VMLMonitorUcStatic.RefreshNodeState(input.NodeGUID,NodeState.WORK,input.TaskId);
                        ViewModelLocator.VMLMonitorUcStatic.RefreshTaskState(task);
                        break;
                    case NodeState.ERROR:
                        ViewModelLocator.VMLMonitorUcStatic.RefreshNodeState(input.NodeGUID, NodeState.ERROR,-1);
                        ViewModelLocator.VMLMonitorUcStatic.CancelTask(task);
                        break;
                }
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
