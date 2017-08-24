using NodeNet.Data;
using NodeNet.Map_Reduce;
using NodeNet.Network.Nodes;
using NodeNet.Network.States;
using NodeNet.Tasks;
using NodeNet.Utilities;
using Swordfish.NET.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly List<Tuple<int, Node>> unidentifiedNodes;
        /* Nombre de noeuds connectés */
        private int nbNodes;

        // Task monitoring nodes */
        Tuple<int, NodeState> monitorTask;

        /* Liste des noeuds connectés */
        public ConcurrentObservableDictionary<string, Node> Nodes { get; set; }

        /* Liste des clients connectés */
        public ConcurrentDictionary<string, Node> Clients { get; set; }

        /* Liste des noeuds connectés */
        public ConcurrentDictionary<int, Tuple<ConcurrentBag<int>, bool>> TaskDistrib { get; set; }

        private Logger Log { get; }

        #endregion

        protected Orchestrator(string name, string address, int port) : base(name, address, port)
        {
            unidentifiedNodes = new List<Tuple<int, Node>>();
            Nodes = new ConcurrentObservableDictionary<string, Node>();
            Clients = new ConcurrentDictionary<string, Node>();
            TaskDistrib = new ConcurrentDictionary<int, Tuple<ConcurrentBag<int>, bool>>();
            WorkerFactory.AddWorker(IdentMethod, new TaskExecutor(this, IdentNode, null, null));
            WorkerFactory.AddWorker(GetCpuMethod, new TaskExecutor(this, ProcessCpuStateOrder, null, null));
            WorkerFactory.AddWorker(TaskStatusMethod, new TaskExecutor(this, RefreshTaskState, null, null));
            Log = new Logger();
        }

        #region Inherited methods

        public async void Listen()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(Address), Port);
            listener.Start();
            Log.Write("Server is listening on port : " + Port, true);
            Console.WriteLine(@"Server is listening on port : " + Port);
            while (true)
            {
                Socket sock = await listener.AcceptSocketAsync();
                DefaultNode connectedNode = new DefaultNode("", ((IPEndPoint)sock.RemoteEndPoint).Address + "", ((IPEndPoint)sock.RemoteEndPoint).Port, sock);
                nbNodes++;
                Log.Write($"Client Connection accepted from {sock.RemoteEndPoint}", true);
                Console.WriteLine($@"Client Connection accepted from {sock.RemoteEndPoint}");
                GetIdentityOfNode(connectedNode);
                Receive(connectedNode);
            }
        }

        private void GetIdentityOfNode(DefaultNode connectedNode)
        {
            int tId = LastTaskId;
            Tuple<int, Node> taskNodeTuple = new Tuple<int, Node>(tId, connectedNode);
            unidentifiedNodes.Add(taskNodeTuple);

            DataInput input = new DataInput()
            {
                Method = IdentMethod,
                NodeGuid = NodeGuid,
                TaskId = tId,
                Data = new Tuple<string, int>(nbNodes.ToString(), connectedNode.Port),
                MsgType = MessageType.Ident
            };
            SendData(connectedNode, input);
        }

        public new void Stop()
        {
            throw new NotImplementedException();
        }

        public void SendDataToAllNodes(DataInput input)
        {
            DataFormater.Serialize(input);
            Console.WriteLine(@"Send Data to " + Nodes.Count + @" Node in orch Nodes list");
            /* Multi Client */
            foreach (var node in Nodes)
            {
                SendData(node.Value, input);
            }
        }

        private void SendDataToAllClients(DataInput input)
        {
            DataFormater.Serialize(input);
            Console.WriteLine(@"Send Data to " + Nodes.Count + @" Node in orch Nodes list");
            /* Multi Client */
            foreach (var node in Clients)
            {
                SendData(node.Value, input);
            }
        }


        public override void ProcessInput(DataInput input, Node node)
        {
            if (input.Method != GetCpuMethod)
            {
                Console.WriteLine(@"Process input for : " + input.Method + @" at : " + DateTime.Now.ToLongTimeString());
            }
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
            object res = executor.DoWork(input);
            if (res == null) return;
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

        #endregion

        #region TaskExecutor method

        public object IdentNode(DataInput data)
        {
            Console.WriteLine(@"Launch Cli");
            foreach (Tuple<int, Node> node in unidentifiedNodes)
            {
                if (node.Item1 == data.TaskId)
                {
                    if (data.ClientGuid != null)
                    {
                        node.Item2.NodeGuid = data.ClientGuid;
                        Console.WriteLine(@"Add Client to list : " + node);
                        Clients.TryAdd(data.ClientGuid, node.Item2);
                        // TODO activer le monitoring pour ce client
                        SendNodesToClient(node.Item2);
                        StartMonitoringForClient(node.Item2);
                    }
                    else if (data.NodeGuid != null)
                    {
                        node.Item2.NodeGuid = data.NodeGuid;
                        if (monitorTask != null)
                        {
                            StartMonitoringForNode(data, node.Item2);
                        }
                        Console.WriteLine(@"Add Node to list : " + node);
                        Nodes.TryAdd(data.NodeGuid, node.Item2);
                        SendNodeToClients(node.Item2);
                    }
                }
            }
            return null;
        }

        private void StartMonitoringForClient(Node client)
        {
            DataInput input = new DataInput()
            {
                Method = GetCpuMethod,
                Data = null,
                ClientGuid = client.NodeGuid,
                NodeGuid = NodeGuid,
                MsgType = MessageType.Call
            };
            ProcessCpuStateOrder(input);
        }

        private void StartMonitoringForNode(DataInput d, Node n)
        {
            DataInput input = new DataInput()
            {
                Method = GetCpuMethod,
                Data = null,
                ClientGuid = d.ClientGuid,
                NodeGuid = NodeGuid,
                MsgType = MessageType.Call,
                TaskId = monitorTask.Item1
            };
            SendData(n, input);
        }

        private object ProcessCpuStateOrder(DataInput input)
        {
            if (input.MsgType == MessageType.Call)
            {
                if (monitorTask == null)
                {
                    int newTaskId = LastTaskId;
                    monitorTask = new Tuple<int, NodeState>(newTaskId, NodeState.Work);
                }
                GetClientFromGuid(input.ClientGuid).Tasks.TryAdd(monitorTask.Item1, new Task(monitorTask.Item1, NodeState.Work));
                input.NodeGuid = NodeGuid;
                input.TaskId = monitorTask.Item1;
                SendDataToAllNodes(input);
            }
            else if (input.MsgType == MessageType.Response && monitorTask != null)
            {
                foreach (var client in Clients)
                {
                    Task monitoringTask;
                    if (client.Value.Tasks.TryGetValue(monitorTask.Item1, out monitoringTask))
                    {
                        SendData(client.Value, input);
                    }
                }
            }
            return null;
        }

        private object RefreshTaskState(DataInput input)
        {
            Console.WriteLine(@"Launch Cli");
            // On fait transiter l'info au client
            SendData(GetClientFromGuid(input.ClientGuid), input);
            // Et on ne renvoit rien au Node
            return null;
        }

        #endregion

        #region Map Reduce
        protected object ProcessMapReduce(DataInput input)
        {
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
            Console.WriteLine(@"Launch Cli");
            switch (input.MsgType)
            {
                case MessageType.Call:
                    LazyNodeTranfert(input);
                    break;
                case MessageType.Response:
                    PrepareReduce(input, executor);
                    break;
            }
            return null;
        }

        private void PrepareReduce(DataInput input, TaskExecutor executor)
        {
            if (TaskIsOk(input.TaskId))
            {
                UpdateNodeTaskStatus(input.NodeTaskId, NodeState.Finish, input.NodeGuid);
                UpdateResult(input.Data, input.TaskId, input.NodeTaskId);
                UpdateNodeStatus(NodeState.Wait, input.NodeGuid);
                // Reduce
                // On cherche l'emplacement du resultat pour cette task et on l'envoit au Reduce 
                // pour y concaténer le resultat du travail du noeud
                ConcurrentBag<object> result = GetResultFromTaskId(input.TaskId);
                if (TaskIsCompleted(input.TaskId))
                {
                    Console.WriteLine(@"Launch Cli");
                    object reduceRes = executor.Reducer.Reduce(result);
                    // TODO check si tous les nodes ont finis
                    DataInput response = new DataInput()
                    {
                        TaskId = input.TaskId,
                        Method = input.Method,
                        Data = reduceRes,
                        ClientGuid = input.ClientGuid,
                        NodeGuid = NodeGuid,
                        MsgType = MessageType.Response,
                    };
                    SendData(GetClientFromGuid(input.ClientGuid), response);
                }
                // Si la tâche n'est pas terminé on envoi sa progession
                else
                {
                    // A condition que le mapping soit terminé, sinon on ne pourra pas savoir combien de noeuds travaillent 
                    if (!TaskDistrib.TryGetValue(input.TaskId, out var taskDist) || !taskDist.Item2) return;
                    double progression = GetProgressionForTask(taskDist);
                    Tuple<NodeState, object> data = new Tuple<NodeState, object>(NodeState.Work, progression);
                    DataInput progress = new DataInput()
                    {
                        TaskId = input.TaskId,
                        Method = TaskStatusMethod,
                        Data = data,
                        ClientGuid = input.ClientGuid,
                        NodeGuid = input.NodeGuid,
                        MsgType = MessageType.Response,
                    };
                    // On envoit la progression de la tâche au client
                    SendData(GetClientFromGuid(input.ClientGuid), progress);
                }
            }
            else
            {
                if (!IsTaskReceiveAllRes(input.TaskId)) return;
                RemoveTask(input.TaskId);
                RemoveResultForTask(input.TaskId);
            }
        }



        private void LazyNodeTranfert(DataInput input)
        {
            int newTaskId = LastTaskId;
            CreateClientTask(input, newTaskId);
            ConcurrentBag<object> emptyResult = new ConcurrentBag<object>();
            Results.TryAdd(newTaskId, emptyResult);
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
            // On récupère le résultat du map
            IMapper mapper = executor.Mapper;
            LaunchMapping(mapper, input, newTaskId);
        }

        /// <summary>
        /// Lance le mapping au sein d'un thread, tout en écoutant les modifications sur la liste des nodes
        /// Si tous les nodes sont occupé, met le thread en attente
        /// Si un noeud passe dans l'état WAIT, relance le mapping
        /// </summary>
        /// <param name="mapper">IMapper permettant le découpage de la donnée</param>
        /// <param name="input">Object contenant la données et les metainformation sur la task</param>
        /// <param name="newTaskId">Id UNIQUE créé pour cette task</param>
        private void LaunchMapping(IMapper mapper, DataInput input, int newTaskId)
        {
            // Démarrage du thread en écoute sur la liste des nodes pour le mapping en lazzy loading
            ManualResetEvent busy = new ManualResetEvent(false);
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
                        if (node.Value.State == NodeState.Wait)
                        {
                            object data = mapper.Map(input.Data,Nodes.Count);
                            // On envoit le resultat du map au node
                            int newNodeTaskId = SendSubTaskToNode(node.Value, newTaskId, input, data);
                            SendNodeIsWorkingToClient(node.Value,newTaskId, newNodeTaskId, input);
                            // On passe son état à WORK
                            node.Value.State = NodeState.Work;
                            if (mapper.MapIsEnd())
                            {
                                // endMap vaudra true si on a déjà tout mapper
                                endMap = true;
                                // Si on a tout mapper on l'indique dans le tableau de distribution des NodeTask
                                SetTaskIsMapped(newTaskId);
                            }

                            allNodeWork = false;
                        }
                    }
                    if (allNodeWork)
                        busy.Reset();
                }
            };

            Nodes.CollectionChanged += delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                // Si il est en train d'attendre on relance le map
                bool unlockThread = false;
                if (e.NewItems != null)
                {
                    foreach (var node in e.NewItems)
                    {
                        if (((KeyValuePair<string, Node>)node).Value.State == NodeState.Wait)
                        {
                            unlockThread = true;
                        }
                    }
                }
                else
                {
                    unlockThread = true;
                }
                if (unlockThread)
                {
                    busy.Set();
                }
            };
            bw.RunWorkerAsync();
        }

        private void SendNodeIsWorkingToClient(Node node, int newTaskId,int newNodeTaskId, DataInput input)
        {
            Tuple<NodeState, object> data = new Tuple<NodeState, object>(NodeState.NodeIsWorking, node.NodeGuid);
            DataInput nodeIsWorking = new DataInput()
            {
                Method = TaskStatusMethod,
                MsgType = MessageType.Response,
                ClientGuid = input.ClientGuid,
                NodeGuid = NodeGuid,
                Data = data,
                TaskId = newTaskId,
                NodeTaskId = newNodeTaskId
            };
            SendData(GetClientFromGuid(input.ClientGuid), nodeIsWorking);
        }

        #endregion

        #region Task Management
        private void SetTaskIsMapped(int newTaskId)
        {
            Tuple<ConcurrentBag<int>, bool> task;
            if (TaskDistrib.TryGetValue(newTaskId, out task))
            {
                Tuple<ConcurrentBag<int>, bool> newTask = new Tuple<ConcurrentBag<int>, bool>(task.Item1, true);
                if (!TaskDistrib.TryUpdate(newTaskId, newTask, task))
                {
                    throw new Exception("Impossible de mettre à jour la tâche pour signifier quelle est mappée.");
                }
            }
            else
            {
                throw new Exception("Aucune Task avec cet id ");
            }
        }

        private int SendSubTaskToNode(Node node, int newTaskId, DataInput input, object data)
        {
            Console.WriteLine(@"Launch Cli");
            int newNodeTaskId = LastSubTaskId;
            CreateNodeTasks(node, newTaskId, newNodeTaskId);
            DataInput res = new DataInput()
            {
                TaskId = newTaskId,
                NodeTaskId = newNodeTaskId,
                MsgType = MessageType.Call,
                Method = input.Method,
                Data = data,
                ClientGuid = input.ClientGuid,
                NodeGuid = NodeGuid,
            };
            SendData(node, res);
            return newNodeTaskId;
        }

        private void CreateClientTask(DataInput input, int newTaskId)
        {
            Task task = new Task(newTaskId, NodeState.JobStart)
            {
                TaskName = input.Method,
                StartTime = DateTime.Now
            };
            // Ajout de la task au client 
            Node client = GetClientFromGuid(input.ClientGuid);
            client.Tasks.TryAdd(newTaskId, task);
            // Ajout d'une ligne dans la table de ditribution des nodeTask
            TaskDistrib.TryAdd(newTaskId, new Tuple<ConcurrentBag<int>, bool>(new ConcurrentBag<int>(), false));
            Tuple<NodeState, object> data = new Tuple<NodeState, object>(NodeState.JobStart, task); 
            DataInput resp = new DataInput()
            {
                ClientGuid = input.ClientGuid,
                NodeGuid = NodeGuid,
                Method = TaskStatusMethod,
                TaskId = newTaskId,
                MsgType = MessageType.Response,
                Data = data
            };
            SendData(client, resp);
            task.State = NodeState.Work;
        }

        private void CreateNodeTasks(Node node, int newTaskId, int newSubTaskId)
        {
            // On ajoute la subtask au node 
            node.Tasks.TryAdd(newSubTaskId, new Task(newSubTaskId, NodeState.Wait));
            // Ajout de la node task à la ligne de la task dans le tableau de distribution des nodetask

            Tuple<ConcurrentBag<int>, bool> task;
            if (TaskDistrib.TryGetValue(newTaskId, out task))
            {
                task.Item1.Add(newSubTaskId);
            }
            else
            {
                throw new Exception("Aucune Task avec cet id ");
            }
        }

        private List<string> GetNodeGuidFromNodeTaskIDs(List<int> list)
        {
            List<string> nodeGuids = new List<string>();
            foreach (int subTaskId in list)
            {
                nodeGuids.Add(GetNodeGuidFromNodeTaskId(subTaskId));
            }
            return nodeGuids;
        }

        private string GetNodeGuidFromNodeTaskId(int subTaskId)
        {
            foreach (var node in Nodes)
            {
                if (node.Value.Tasks.TryGetValue(subTaskId, out Task _))
                {
                    return node.Value.NodeGuid;
                }
            }
            throw new Exception("Aucun nodeGUID trouvé pour cet id de Nodetask");
        }

        private Task GetClientTask(int taskId, string clientGuid)
        {
            Node client = GetClientFromGuid(clientGuid);
            if (client.Tasks.TryGetValue(taskId, out Task task))
            {
                return task;
            }
            throw new Exception("Aucune task trouvé pour ce client et cet id");
        }

        private double GetProgressionForTask(Tuple<ConcurrentBag<int>, bool> taskDist)
        {
            int nbNodeWork = taskDist.Item1.Count();
            int nbNodeEnded = 0;
            foreach (var node in Nodes)
            {
                foreach (int idNodeTask in taskDist.Item1)
                {
                    if (node.Value.Tasks.TryGetValue(idNodeTask, out Task task) && task.State == NodeState.Finish)
                    {
                        nbNodeEnded++;
                    }
                }
            }
            return nbNodeEnded * 100 / nbNodeWork;
        }

        private Task UpdateTaskStatus(int id, NodeState status)
        {
            foreach (var client in Clients)
            {
                Task task;
                if (client.Value.Tasks.TryGetValue(id, out task))
                {
                    Task newTask = new Task(id, status) {StartTime = task.StartTime};
                    if (status == NodeState.Error)
                    {
                        // On signifie à la tâche qu'elle est terminée
                        newTask.EndTime = DateTime.Now;
                    }
                    newTask.Progression = task.Progression;
                    newTask.TaskName = task.TaskName;
                    client.Value.Tasks.TryUpdate(id, newTask, task);
                    return newTask;
                }
            }
            throw new Exception("Aucune tâche n'a pu etre mise à jour");
        }


        private void UpdateNodeTaskStatus(int nodeTaskId, NodeState status, string nodeGuid)
        {
            Node node;
            if (Nodes.TryGetValue(nodeGuid, out node))
            {
                Task task;
                if (node.Tasks.TryGetValue(nodeTaskId, out task))
                {
                    Task newTask = new Task(task.Id, status) {TaskName = task.TaskName};
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

        private void UpdateNodeStatus(NodeState status, string nodeGuid)
        {
            Console.WriteLine(@"Set status of node : " + nodeGuid + @" to : " + status);
            Node node;
            if (Nodes.TryGetValue(nodeGuid, out node))
            {
                node.State = status;
            }
            else
            {
                throw new Exception("Aucun Node trouvé avec ce GUID");
            }
        }

        private bool TaskIsOk(int taskId)
        {
            Node client = GetClientFromTaskId(taskId);
            return GetTaskState(taskId) != NodeState.Error && client.State != NodeState.Dead;
        }

        #endregion

        #region Utilitary methods
        private void SendNodeToClients(Node n)
        {
            List<List<string>> monitoringValues = new List<List<string>>
            {
                GetMonitoringInfos(n)
            };
            foreach (KeyValuePair<string, Node> client in Clients)
            {
                DataInput di = new DataInput()
                {
                    ClientGuid = client.Value.NodeGuid,
                    NodeGuid = NodeGuid,
                    Method = IdentMethod,
                    Data = monitoringValues,
                    MsgType = MessageType.NodeIdent
                };
                SendData(client.Value, di);

            }
        }

        protected Node GetNodeFromGuid(string guid)
        {
            return Nodes.TryGetValue(guid, out Node node) ? node : null;
        }


        protected Node GetClientFromGuid(string guid)
        {
            return (from client in Clients where client.Value.NodeGuid.Equals(guid) select client.Value).FirstOrDefault();
        }

        public void SendNodesToClient(Node client)
        {
            //TODO check Enumerator Thread safe
            List<List<string>> monitoringValues = new List<List<string>>();
            IEnumerator<KeyValuePair<string, Node>> en = Nodes.GetEnumerator();
            while (en.MoveNext())
            {
                KeyValuePair<string, Node> node = en.Current;
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
                    ClientGuid = client.NodeGuid,
                    NodeGuid = NodeGuid,
                    Method = IdentMethod,
                    Data = monitoringValues,
                    MsgType = MessageType.NodeIdent
                };
                SendData(client, di);
            }
        }

        private Node GetNodeBySubTaskId(int subtaskId)
        {
            foreach (var node in Nodes)
            {
                Task task;
                if (node.Value.Tasks.TryGetValue(subtaskId, out task))
                {
                    return node.Value;
                }
            }
            throw new Exception("Aucune node associé à cette subTask n'a été trouvé");
        }

        private NodeState GetTaskState(int taskId)
        {
            foreach (var client in Clients)
            {
                Task task;
                if (client.Value.Tasks.TryGetValue(taskId, out task))
                {
                    return task.State;
                }
            }
            throw new Exception("Aucune Task avec cet ID n'a été trouvée");
        }

        private NodeState GetSubTaskState(int subtaskId)
        {
            foreach (var node in Nodes)
            {
                Task task;
                if (node.Value.Tasks.TryGetValue(subtaskId, out task))
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
                Console.WriteLine(@"Task is mapped : " + task.Item2);
                if (task.Item2)
                {
                    // On itère sur toute la liste des subtask de cette task
                    foreach (int subTask in task.Item1)
                    {
                        NodeState state = GetSubTaskState(subTask);
                        Console.WriteLine(@"InTaskComplete state of subTask : " + subTask + @" : " + state);
                        // Ici on vérifie qsue toutes les subtask soient en état FINISH à part celle que l'on vient de recevoir
                        if (state != NodeState.Finish)
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
            Console.WriteLine(@"In TaskIsComplete -> complete : " + completed + @" Thread : " + Thread.CurrentThread.ManagedThreadId);
            return completed;
        }

        private void RemoveResultForTask(int taskId)
        {
            if (!Results.TryRemove(taskId, out ConcurrentBag<object> _))
            {
                throw new Exception("Aucune Result avec ce task id");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public override void RemoveDeadNode(Node node)
        {
            // On passe le noeud à l'état DEAD
            node.State = NodeState.Dead;
            // On vérifier si c'est un Node ou un Client
            if (GetNodeFromGuid(node.NodeGuid) != null)
            {
                List<Task> taskError = SetNodeTaskToError(node,false);
                Tuple<NodeState, object> data = new Tuple<NodeState, object>(NodeState.Error, taskError);
                DataInput errorInput = new DataInput()
                {
                    Method = TaskStatusMethod,
                    NodeGuid = node.NodeGuid,
                    MsgType = MessageType.Call,
                    Data = data
                };
                // Envoi du status aux clients
                SendDataToAllClients(errorInput);
                // On supprime le node
                Nodes.Remove(node.NodeGuid);

            }
            else if (GetClientFromGuid(node.NodeGuid) != null)
            {
                SetNodeTaskToError(node, true);
                // On supprime le node
                Clients.TryRemove(node.NodeGuid,out var _);
            }

        }

        public List<Task> SetNodeTaskToError(Node node,bool isClient)
        {
            List<Task> errorTasks = new List<Task>();
            List<int> tasks;
            List<int> subTasks;
            if (isClient)
            {
                tasks = node.Tasks.Keys.ToList();
                subTasks = GetAllNodeTaskIdForClient(tasks);
            }
            else
            {
                tasks = GetAllClientTaskIdFromNode(node);
                subTasks = node.Tasks.Keys.ToList();
            }
            // On passe le statut des NodeTask à ERROR
            foreach (int id in subTasks)
            {
                UpdateNodeTaskStatus(id, NodeState.Error, node.NodeGuid);
            }
            foreach (int id in tasks)
            {
                // On stoppe le mapping pour toutes les tasks
                SetTaskIsMapped(id);
                errorTasks.Add(UpdateTaskStatus(id, NodeState.Error));
            }
            return errorTasks;
        }


        private List<int> GetAllNodeTaskIdForClient(List<int> taskIds)
        {
            List<int> nodeTaskId = new List<int>();
            foreach(int id in taskIds)
            {
                foreach (var taskDist in TaskDistrib)
                {
                    if(taskDist.Key == id)
                    {
                        nodeTaskId.AddRange(taskDist.Value.Item1);
                    }
                }
            }
            
            return nodeTaskId;
        }

        private List<int> GetAllClientTaskIdFromNode(Node node)
        {
            List<int> tasks = new List<int>();
            List<int> taskIDs = node.Tasks.Keys.ToList();
            foreach (int unused in taskIDs)
            {
                foreach (KeyValuePair<int, Tuple<ConcurrentBag<int>, bool>> task in TaskDistrib)
                {
                    tasks.Add(task.Key);
                }
            }
            return tasks;
        }

        private Node GetClientFromTaskId(int taskId)
        {
            foreach(var client in Clients)
            {   
                if(client.Value.Tasks.TryGetValue(taskId,out var _))
                {
                    return client.Value;
                }
            }
            throw new Exception("Aucun Client trouvé pour ce task id");
        }

        private void RemoveTask(int taskId)
        {
            // On supprime la ligne dans le tableau de distribution des Task et NodeTask TaskDistrib
            if (TaskDistrib.TryRemove(taskId, out var taskDistrib))
            {
                foreach (var client in Clients)
                {
                    // On supprime la task dans le client
                    client.Value.Tasks.TryRemove(taskId, out var _);
                }
                foreach (var node in Nodes)
                {
                    // On supprime les nodeTask dans les node
                    foreach (int nodeTaskId in taskDistrib.Item1)
                    {
                        node.Value.Tasks.TryRemove(nodeTaskId, out Task _);
                    }
                }
            }
            else
            {
                throw new Exception("Impossible de supprimer la ligne de task distrib après une erreur sur un Node");
            }
        }

        private bool IsTaskReceiveAllRes(int taskId)
        {
            bool allResReceived = true;
            Tuple<ConcurrentBag<int>, bool> subTasks;
            if (TaskDistrib.TryGetValue(taskId, out subTasks))
            {
                foreach (int subId in subTasks.Item1)
                {
                    if (GetSubTaskState(subId) == NodeState.Wait)
                    {
                        allResReceived = false;
                    }
                }
            }
            return allResReceived;
        }
        #endregion
    }
}