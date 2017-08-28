using NodeNet.Data;
using NodeNet.GUI.ViewModel;
using NodeNet.Network.Nodes;
using NodeNet.Network.States;
using NodeNet.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using NodeNet.Utilities;

namespace NodeNet.Network
{
    /// <summary>
    /// Objet définnissant le comportement du Client
    /// </summary>
    public abstract class DefaultClient : Node
    {
        /// <summary>
        /// Constructeur initialisant le nom l'adresse IP et le port d'écoute ainsi que les TaskExecutor associés aux méthodes infrastructures
        /// </summary>
        /// <param name="name">nom du client</param>
        /// <param name="adress">adresse IP</param>
        /// <param name="port">port d'écoute</param>
        protected DefaultClient(string name, string adress, int port) : base(name,adress,port)
        {
            Logger = new Logger(true);
            WorkerFactory.AddWorker(GetCpuMethod, new TaskExecutor(this,RefreshCpuState,null,null));
            WorkerFactory.AddWorker(IdentMethod, new TaskExecutor(this,ProcessIdent,null,null));
            WorkerFactory.AddWorker(TaskStatusMethod, new TaskExecutor(this, RefreshTaskState, null, null));
        }


        /// <summary>
        /// Gère la reception des demandes contenues dans l'objet de transfert depuis un noeud
        /// </summary>
        /// <param name="input">objet de transfert</param>
        /// <param name="node">noeud emetteur</param>
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

        /// <summary>
        /// Rafraichit la vue avec les informations de l'objet de transfert
        /// </summary>
        /// <param name="input">objet de transfert</param>
        /// <returns>null</returns>
        public object RefreshCpuState(DataInput input)
        {
            ViewModelLocator.VmlMonitorUcStatic.RefreshNodesInfo(input);
            return null;
        }



        /// <summary>
        /// Ajoute les informations des noeuds à la liste des informations de monitoring des noeuds
        /// </summary>
        /// <param name="monitoringValues">Liste des informations de monitoring des noeuds</param>
        public void AddNodeToList(List<List<string>> monitoringValues)
        {
            foreach (List<string> nodeinfo in monitoringValues)
            {
                Logger.Write($"Ajout d'un node dans la liste ({nodeinfo[0]} - {nodeinfo[1]} - {nodeinfo[2]})");
                DefaultNode n = new DefaultNode(nodeinfo[0], nodeinfo[1], Convert.ToInt32(nodeinfo[2]), false);
                Application.Current.Dispatcher.Invoke(() => ViewModelLocator.VmlMonitorUcStatic.NodeList.Add(n));      
            }
        }

        /// <summary>
        /// Gère une requête d'identification depuis un objet de transfert
        /// </summary>
        /// <param name="input">objet de transfert</param>
        /// <returns></returns>
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

        /// <summary>
        /// Rafraichit visuellement l'état d'une tâche depuis les informations contenues dans un objet de transfert
        /// </summary>
        /// <param name="input">objet de transfert</param>
        /// <returns>null</returns>
        private object RefreshTaskState(DataInput input)
        {
           Tuple<NodeState,object> state = (Tuple<NodeState,object>)input.Data;
            switch (state.Item1)
            {
                case NodeState.JobStart:
                    Logger.Write($"Ordre reçu par l'orchestrateur pour la tâche {((Task)state.Item2).TaskName} avec l'id {((Task)state.Item2).Id}");
                    ViewModelLocator.VmlMonitorUcStatic.CreateTask((Task)state.Item2);
                    break;
                case NodeState.NodeIsWorking:
                    Logger.Write($"Démarrage de l'exécution de la tâche avec l'id {input.TaskId}");
                    ViewModelLocator.VmlMonitorUcStatic.NodeIsWorkingOnTask((string)state.Item2,input.TaskId);
                break;
                case NodeState.Work:
                    ViewModelLocator.VmlMonitorUcStatic.RefreshNodesState(input.NodeGuid, NodeState.Finish);
                    ViewModelLocator.VmlMonitorUcStatic.RefreshTaskState(input.TaskId, (double)state.Item2);
                    break;
                case NodeState.Error:
                    Logger.Write($"Erreur : abandon de la tâche avec l'id {input.TaskId}");
                    ViewModelLocator.VmlMonitorUcStatic.NodeIsFailed(input.NodeGuid);
                    ViewModelLocator.VmlMonitorUcStatic.CancelTask(input.Data as List<Task>);
                    break;
            }
            return null;
        }

        /// <summary>
        /// Supprime un noeud arrêté sur la vue
        /// </summary>
        /// <param name="node">noeud arrêté</param>
        public override void RemoveDeadNode(Node node)
        {
            MessageBox.Show("Erreur sur l'orchestrateur. Fermeture de l'application...");
            Process.GetCurrentProcess().CloseMainWindow();
        }
    }
}
