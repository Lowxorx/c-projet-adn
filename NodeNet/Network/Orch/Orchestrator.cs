using NodeNet.Data;
using NodeNet.Map_Reduce;
using NodeNet.Network.Nodes;
using NodeNet.Network.States;
using NodeNet.Tasks;
using NodeNet.Utilities;
using Swordfish.NET.Collections;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NodeNet.Network.Orch
{
    public abstract class Orchestrator : Node, IOrchestrator
    {
        #region Properties
        private List<Tuple<int, Node>> UnidentifiedNodes;
        /* Nombre de noeuds connectés */
        private int nbNodes = 0;
       
        // Task monitoring nodes */
        Tuple<int, NodeState> MonitorTask;

        /* Liste des noeuds connectés */
        private ConcurrentObservableDictionary<String,Node> nodes;
        public ConcurrentObservableDictionary<String,Node> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }

        /* Liste des clients connectés */
        private ConcurrentDictionary<String, Node> clients;
        public ConcurrentDictionary<String, Node> Clients
        {
            get { return clients; }
            set { clients = value; }
        }

        /* Liste des noeuds connectés */
        private ConcurrentDictionary<int,Tuple<ConcurrentBag<int>,bool>> taskDistrib;
        public ConcurrentDictionary<int, Tuple<ConcurrentBag<int>, bool>> TaskDistrib
        {
            get { return taskDistrib; }
            set { taskDistrib = value; }
        }

        private Logger Log { get; set; }

        #endregion

        public Orchestrator(string name, string address, int port) : base(name, address, port)
        {
            UnidentifiedNodes = new List<Tuple<int, Node>>();
            Nodes = new ConcurrentObservableDictionary<String, Node>();
            Clients = new ConcurrentDictionary<String,Node>();
            TaskDistrib = new ConcurrentDictionary<int, Tuple<ConcurrentBag<int>, bool>>();
            WorkerFactory.AddWorker(IDENT_METHOD, new TaskExecutor(this,IdentNode,null,null));
            WorkerFactory.AddWorker(GET_CPU_METHOD, new TaskExecutor(this,ProcessCPUStateOrder,null,null));
            WorkerFactory.AddWorker(TASK_STATUS_METHOD, new TaskExecutor(this, RefreshTaskState, null, null));
            Log = new Logger();
        }

        #region Inherited methods

        public async void Listen()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(Address), Port);
            listener.Start();
            Log.Write("Server is listening on port : " + Port);
            Console.WriteLine("Server is listening on port : " + Port);
            while (true)
            {
                Socket sock = await listener.AcceptSocketAsync();
                DefaultNode connectedNode = new DefaultNode("", ((IPEndPoint)sock.RemoteEndPoint).Address + "", ((IPEndPoint)sock.RemoteEndPoint).Port, sock);
                nbNodes++;
                Log.Write(String.Format("Client Connection accepted from {0}", sock.RemoteEndPoint.ToString()));
                Console.WriteLine(String.Format("Client Connection accepted from {0}", sock.RemoteEndPoint.ToString()));
                GetIdentityOfNode(connectedNode);
                Receive(connectedNode);
            }
        }

        private void GetIdentityOfNode(DefaultNode connectedNode)
        {
            int tId = LastTaskID;
            Tuple<int, Node> taskNodeTuple = new Tuple<int, Node>(tId, connectedNode);
            UnidentifiedNodes.Add(taskNodeTuple);

            DataInput input = new DataInput()
            {
                Method = IDENT_METHOD,
                NodeGUID = NodeGUID,
                TaskId = tId,
                Data = new Tuple<String,int>(nbNodes.ToString(),connectedNode.Port),
                MsgType = MessageType.IDENT
            };
            SendData(connectedNode, input);
        }

        public new void Stop()
        {
            throw new NotImplementedException();
        }

        public void SendDataToAllNodes(DataInput input)
        {
            byte[] data = DataFormater.Serialize(input);
            Console.WriteLine("Send Data to " + Nodes.Count + " Node in orch Nodes list");
            /* Multi Client */
            foreach (var node in Nodes)
            {
                try
                {
                    SendData(node.Value, input);
                }
                catch (SocketException ex)
                {
                    /// Client Down ///
                    if (!node.Value.NodeSocket.Connected)
                    {
                        Console.WriteLine("Client " + node.Value.NodeSocket.RemoteEndPoint.ToString() + " Disconnected");
                    }
                    Console.WriteLine(ex.ToString());
                }
            }
        }

      
        public override void ProcessInput(DataInput input, Node node)
        {
            if (input.Method != GET_CPU_METHOD)
            {
                Console.WriteLine("Process input for : " + input.Method + " at : " + DateTime.Now.ToLongTimeString()) ;
            }
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
            Object res = executor.DoWork(input);
            if(res != null) {
                DataInput resp = new DataInput()
                {
                    ClientGUID = input.ClientGUID,
                    NodeGUID = NodeGUID,
                    TaskId =input.TaskId,
                    Method = input.Method,
                    Data = res,
                    MsgType = MessageType.RESPONSE
                };
                SendData(node, resp);
            }
        }

        #endregion

        #region TaskExecutor method

        public Object IdentNode(DataInput data)
        {
            Console.WriteLine("Process Ident On Orch");
            foreach (Tuple<int, Node> node in UnidentifiedNodes)
            {
                if (node.Item1 == data.TaskId)
                {
                    if (data.ClientGUID != null)
                    {
                        node.Item2.NodeGUID = data.ClientGUID;
                        Console.WriteLine("Add Client to list : " + node);
                        Clients.TryAdd(data.ClientGUID,node.Item2);
                        // TODO activer le monitoring pour ce client
                        SendNodesToClient(node.Item2);
                        startMonitoringForClient(node.Item2);
                    }
                    else if (data.NodeGUID != null)
                    {
                        node.Item2.NodeGUID = data.NodeGUID;
                        if (MonitorTask != null)
                        {
                            startMonitoringForNode(data, node.Item2);
                        }
                        Console.WriteLine("Add Node to list : " + node);
                        Nodes.TryAdd(data.NodeGUID,node.Item2);
                        SendNodeToClients(node.Item2);
                    }
                }
            }
            return null;
        }

        private void startMonitoringForClient(Node client)
        {
            DataInput input = new DataInput()
            {
                Method = GET_CPU_METHOD,
                Data = null,
                ClientGUID = client.NodeGUID,
                NodeGUID = NodeGUID,
                MsgType = MessageType.CALL
            };
            ProcessCPUStateOrder(input);
        }

        private void startMonitoringForNode(DataInput d, Node n)
        {
            DataInput input = new DataInput()
            {
                Method = GET_CPU_METHOD,
                Data = null,
                ClientGUID = d.ClientGUID,
                NodeGUID = NodeGUID,
                MsgType = MessageType.CALL,
                TaskId = MonitorTask.Item1
            };
            SendData(n, input);
        }

        private Object ProcessCPUStateOrder(DataInput input)
        {
            if (input.MsgType == MessageType.CALL)
            {
                if (MonitorTask == null)
                {
                    int newTaskID = LastTaskID;
                    MonitorTask = new Tuple<int, NodeState>(newTaskID, NodeState.WORK);
                }
                GetClientFromGUID(input.ClientGUID).Tasks.TryAdd(MonitorTask.Item1,new Task(MonitorTask.Item1,NodeState.WORK));
                input.NodeGUID = NodeGUID;
                input.TaskId = MonitorTask.Item1;
                SendDataToAllNodes(input);
            }
            else if (input.MsgType == MessageType.RESPONSE && MonitorTask != null)
            {
                foreach ( var client in Clients)
                {
                    Task monitoringTask;
                    if(client.Value.Tasks.TryGetValue(MonitorTask.Item1,out monitoringTask))
                    {
                        SendData(client.Value, input);
                    }
                }
            }
            return null;
        }

        private object RefreshTaskState(DataInput input)
        {
           Console.WriteLine("Process RefreshTaskState ");
           // On fait transiter l'info au client
           SendData(GetClientFromGUID(input.ClientGUID), input);
           // Et on ne renvoit rien au Node
            return null;
        }

        #endregion

        #region Map Reduce
        protected Object ProcessMapReduce(DataInput input)
        {
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
            Console.WriteLine("Process Map/Reduce");
            if (input.MsgType == MessageType.CALL)
            {
                LazyNodeTranfert(input);
            }
            else if (input.MsgType == MessageType.RESPONSE)
            {   
                PrepareReduce(input,executor);
            }
            return null;
        }

       // [MethodImpl(MethodImplOptions.Synchronized)]
        private void PrepareReduce(DataInput input, TaskExecutor executor)
        {
            updateNodeTaskStatus(input.NodeTaskId, NodeState.FINISH, input.NodeGUID);
            UpdateResult(input.Data, input.TaskId);
            updateNodeStatus(NodeState.WAIT, input.NodeGUID);
            // Reduce
            // On cherche l'emplacement du resultat pour cette task et on l'envoit au Reduce 
            // pour y concaténer le resultat du travail du noeud
            ConcurrentBag<Object> result = GetResultFromTaskId(input.TaskId);
            if (TaskIsCompleted(input.TaskId))
            {
                Console.WriteLine("Reduce");
                Object reduceRes = executor.Reducer.reduce(result);
                // TODO check si tous les nodes ont finis
                DataInput response = new DataInput()
                {
                    TaskId = input.TaskId,
                    Method = input.Method,
                    Data = reduceRes,
                    ClientGUID = input.ClientGUID,
                    NodeGUID = NodeGUID,
                    MsgType = MessageType.RESPONSE,
                };
                SendData(GetClientFromGUID(input.ClientGUID), response);
            }
        }

        private void LazyNodeTranfert(DataInput input)
        {
            int newTaskId = LastTaskID;
            createClientTask(input, newTaskId);
            ConcurrentBag<Object> emptyResult = new ConcurrentBag<Object>();
            Results.TryAdd(newTaskId,emptyResult);
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
            // On récupère le résultat du map
            IMapper mapper = executor.Mapper;
            // Démarrage du thread en écoute sur la liste des nodes pour le mapping en lazzy loading
            ManualResetEvent _busy = new ManualResetEvent(false);
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            bw.DoWork += (o, a) =>
            {
                bool endMap = false;
               
                while (!endMap)
                {
                    bool allNodeWork = true;
                    // On itère sur la liste des Nodes pour leur distribuer du travail
                    foreach (var node in Nodes)
                    {
                        // Si au moins un node de la liste est en train d'attendre 
                        // on lui envoit du travail
                        if (node.Value.State == NodeState.WAIT)
                        {
                            Object data = mapper.map(input.Data);
                           
                            sendSubTaskToNode(node.Value, newTaskId, input, data);
                            node.Value.State = NodeState.WORK;
                            if (mapper.mapIsEnd())
                            {
                                // endMap vaudra true si on a déjà tout mapper
                                endMap = true;
                                // Si on a tout mapper on l'indique dans le tableau de distribution des NodeTask
                                setTaskIsMapped(newTaskId);
                            }

                            allNodeWork = false;
                        }      
                    }
                    if (allNodeWork)
                        _busy.Reset();
                }
            };

            Nodes.CollectionChanged += delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                // Si il est en train d'attendre on relance le map
                bool oneNodeIsWaiting = false;
                foreach(var node in e.NewItems)
                {
                    if (((KeyValuePair<String,Node>)node).Value.State == NodeState.WAIT)
                    {
                        oneNodeIsWaiting = true;
                    }
                }
                if (oneNodeIsWaiting)
                {
                    _busy.Set();
                }
            };
            bw.RunWorkerAsync();
        }

        #endregion

        #region Task Management
        private void setTaskIsMapped(int newTaskId)
        {
            Tuple<ConcurrentBag<int>, bool> task;
            if (TaskDistrib.TryGetValue(newTaskId, out task))
            {
                Tuple<ConcurrentBag<int>, bool> newTask = new Tuple<ConcurrentBag<int>, bool>(task.Item1, true);
                if(!TaskDistrib.TryUpdate(newTaskId, newTask, task))
                {
                    throw new Exception("Impossible de mettre à jour la tâche pour signifier quelle est mappée.");
                }
            }
            else{
                throw new Exception("Aucune Task avec cet id ");
            }
        }

        private void sendSubTaskToNode(Node node, int newTaskID, DataInput input, Object data)
        {
            Console.WriteLine("SendMApToNode");
            int newNodeTaskID = LastSubTaskID;
            createNodeTasks(node, newTaskID, newNodeTaskID);
            DataInput res = new DataInput()
            {
                TaskId = newTaskID,
                NodeTaskId = newNodeTaskID,
                MsgType = MessageType.CALL,
                Method = input.Method,
                Data = data,
                ClientGUID = input.ClientGUID,
                NodeGUID = this.NodeGUID,
            };
            SendData(node, res);
        }

        private void createClientTask(DataInput input, int newTaskID)
        {
            // Ajout de la task au client 
            Node client = GetClientFromGUID(input.ClientGUID);
            client.Tasks.TryAdd(newTaskID, new Task(newTaskID, NodeState.WAIT));
            // Ajout d'une ligne dans la table de ditribution des nodeTask
            TaskDistrib.TryAdd(newTaskID,new Tuple<ConcurrentBag<int>, bool>(new ConcurrentBag<int>(),false));
            // On envoit une réponse au client pour lui transmettre l'ID de la Task
            DataInput resp = new DataInput()
            {
                ClientGUID = input.ClientGUID,
                NodeGUID = NodeGUID,
                Method = TASK_STATUS_METHOD,
                TaskId = newTaskID,
                MsgType = MessageType.RESPONSE,
                Data = new Tuple<NodeState, Object>(NodeState.JOB_START, input.Method)
            };
            SendData(client, resp);
        }

        private void createNodeTasks( Node node, int newTaskID, int newSubTaskID)
        {
            // On ajoute la subtask au node 
            node.Tasks.TryAdd(newSubTaskID,new Task(newSubTaskID, NodeState.WAIT));
            // Ajout de la node task à la ligne de la task dans le tableau de distribution des nodetask

            Tuple<ConcurrentBag<int>, bool> task;
            if (TaskDistrib.TryGetValue(newTaskID, out task))
            {
                task.Item1.Add(newSubTaskID);
            }
            else
            {
                throw new Exception("Aucune Task avec cet id ");
            }
        }

        private void updateNodeTaskStatus(int nodeTaskId, NodeState status, string nodeGUID)
        {
            Node node;
            if (Nodes.TryGetValue(nodeGUID, out node))
            {
                Task task;
                if (node.Tasks.TryGetValue(nodeTaskId, out task))
                {
                    Task newTask= new Task(task.Id,NodeState.FINISH);
                    newTask.TaskName = task.TaskName;
                    if (!node.Tasks.TryUpdate(nodeTaskId, newTask, task))
                    {
                        throw new Exception("Impossible de mettre à jour la tâche pour signifier quelle est mappée.");
                    }
                }
                else
                {
                    throw new Exception("Aucune Task trouvé avec ce GUID");
                }
            }
            else
            {
                throw new Exception("Aucun Node trouvé avec ce GUID");
            }
        }

        private void updateNodeStatus(NodeState status, string nodeGUID)
        {
            Console.WriteLine("Set status of node : " + nodeGUID + " to : " + status);
            Node node;
            if(Nodes.TryGetValue(nodeGUID,out node))
            {
                node.State = status;
            }
            else
            {
                throw new Exception("Aucun Node trouvé avec ce GUID");
            }
        }

        #endregion

        #region Utilitary methods
        private void SendNodeToClients(Node n)
        {
            List<List<String>> monitoringValues = new List<List<String>>
            {
                GetMonitoringInfos(n)
            };
            foreach (var client in Clients)
            {
                DataInput di = new DataInput()
                {
                    ClientGUID = client.Value.NodeGUID,
                    NodeGUID = NodeGUID,
                    Method = IDENT_METHOD,
                    Data = monitoringValues,
                    MsgType = MessageType.NODE_IDENT
                };
                SendData(client.Value, di);

            }
        }

        protected Node GetNodeFromGUID(String guid)
        {
            Node node;
            if (Nodes.TryGetValue(guid, out node))
            {
                return node;
            }
            else
            {
                throw new Exception("Aucon node trouvé avec ce GUID");
            }
        }


        protected Node GetClientFromGUID(String guid)
        {
            foreach (var client in Clients)
            {
                if (client.Value.NodeGUID.Equals(guid))
                {
                    return client.Value;
                }
            }
            throw new Exception("Aucun client trouvé avec ce guid");
        }

        public void SendNodesToClient(Node client)
        {
            //TODO check Enumerator Thread safe
            List<List<String>> monitoringValues = new List<List<String>>();
            IEnumerator<KeyValuePair<String,Node>> en = Nodes.GetEnumerator();
            while (en.MoveNext())
            {
                KeyValuePair<String, Node> node = en.Current;
                List<string> l = GetMonitoringInfos(node.Value);
                if (l != null)
                {
                    monitoringValues.Add(l);
                }
            }


            if (monitoringValues.Count > 0)
            {
                DataInput di = new DataInput()
                {
                    ClientGUID = client.NodeGUID,
                    NodeGUID = NodeGUID,
                    Method = IDENT_METHOD,
                    Data = monitoringValues,
                    MsgType = MessageType.NODE_IDENT
                };
                SendData(client, di);
            }
        }

        private Node getNodeBySubTaskId(int subtaskID)
        {
            foreach (var node in Nodes)
            {
                Task task;
                if(node.Value.Tasks.TryGetValue(subtaskID, out task))
                {
                    return node.Value;
                }
            }
            throw new Exception("Aucune node associé à cette subTask n'a été trouvé");
        }

        private NodeState getSubTaskState(int subtaskID)
        {
            foreach (var node in Nodes)
            {
                Task task;
                if (node.Value.Tasks.TryGetValue(subtaskID, out task))
                {
                    return task.State;
                }
            }
            throw new Exception("Aucune task trouvé pour cette subTask");
        }

        // Checker si toutes les nodes correspondant à cette task sont en etat FINISH
        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool TaskIsCompleted(int taskId)
        {
            bool completed = true;

            Tuple<ConcurrentBag<int>, bool> task;
            if (TaskDistrib.TryGetValue(taskId, out task))
            {
                // Si le booleen vaut true c'est que le mapping est terminé 
                // donc on peut vérifier si toutes les subtask ont été process
                Console.WriteLine("Task is mapped : " + task.Item2)
;                if (task.Item2)
                {
                    // On itère sur toute la liste des subtask de cette task
                    foreach(int subTask in task.Item1)
                    {
                        NodeState state = getSubTaskState(subTask);
                        Console.WriteLine("InTaskComplete state of subTask : " + subTask + " : " + state);
                        // Ici on vérifie qsue toutes les subtask soient en état FINISH à part celle que l'on vient de recevoir
                        if (state != NodeState.FINISH)
                        {
                            completed = false;
                        }
                    }
                }
                // Si le booleen vaut false pas la peine de vérifier
                else
                {
                    completed = false;
                }
            }
            else
            {
                throw new Exception("Aucune Task avec cet id ");
            }
            Console.WriteLine("In TaskIsComplete -> complete : " + completed + " Thread : " + Thread.CurrentThread.ManagedThreadId);
            return completed;
        }
        #endregion
    }
}