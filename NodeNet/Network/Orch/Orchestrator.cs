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
    /// <summary>
    /// Classe Orchestrateur chargée de transmettre les tâches demandées par le client aux différents noeuds, d'identifier 
    /// et de se connecter aux noeuds
    /// </summary>
    public abstract class Orchestrator : Node, IOrchestrator
    {
        #region Properties
        /// <summary>
        /// 
        /// </summary>
        private readonly List<Tuple<int, Node>> unidentifiedNodes;
        /// <summary>
        /// Nombre de noeuds connectés 
        /// </summary>
        private int nbNodes;

        /// <summary>
        /// ID de la tâche de monitoring des noeuds
        /// </summary>
        Tuple<int, NodeState> monitorTask;

        /// <summary>
        /// Liste des noeuds connectés à cet orchestrateur
        /// </summary>
        public ConcurrentObservableDictionary<string, Node> Nodes { get; set; }

        /// <summary>
        /// Liste des clients connectés à cet orchestrateur
        /// </summary>
        public ConcurrentDictionary<string, Node> Clients { get; set; }

        /// <summary>
        /// Liste des associations des tâches
        /// </summary>
        public ConcurrentDictionary<int, Tuple<ConcurrentBag<int>, bool>> TaskDistrib { get; set; }

        #endregion
        /// <summary>
        /// Constructeur initialisant les listes de noeuds et clients et générant les TaskExecutor infrastructure
        /// </summary>
        /// <param name="name">Nom Orchestrateur</param>
        /// <param name="address">Adresse IP de l'orchestrateur</param>
        /// <param name="port">Port d'écoute</param>
        protected Orchestrator(string name, string address, int port) : base(name, address, port)
        {
            Logger = new Logger(true);
            unidentifiedNodes = new List<Tuple<int, Node>>();
            Nodes = new ConcurrentObservableDictionary<string, Node>();
            Clients = new ConcurrentDictionary<string, Node>();
            TaskDistrib = new ConcurrentDictionary<int, Tuple<ConcurrentBag<int>, bool>>();
            WorkerFactory.AddWorker(IdentMethod, new TaskExecutor(this, IdentNode, null, null));
            WorkerFactory.AddWorker(GetCpuMethod, new TaskExecutor(this, ProcessCpuStateOrder, null, null));
            WorkerFactory.AddWorker(TaskStatusMethod, new TaskExecutor(this, RefreshTaskState, null, null));
        }

        #region Inherited methods

        /// <summary>
        /// Méthode d'écoute par Socket asynchrone 
        /// </summary>
        public async void Listen()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(Address), Port);
            listener.Start();
            Logger.Write($"Serveur en écoute sur le port {Port}");
            while (true)
            {
                Socket sock = await listener.AcceptSocketAsync();
                DefaultNode connectedNode = new DefaultNode("", ((IPEndPoint)sock.RemoteEndPoint).Address + "", ((IPEndPoint)sock.RemoteEndPoint).Port, sock);
                nbNodes++;
                Logger.Write($"Connexion acceptée depuis l'adresse {sock.RemoteEndPoint}");
                GetIdentityOfNode(connectedNode);
                Receive(connectedNode);
            }
        }

        /// <summary>
        /// Méthode d'identification de noeud lors d'une connexion entrante
        /// </summary>
        /// <param name="connectedNode"></param>
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



        /// <summary>
        /// Métode appelant l'envoi de données à tous les noeuds connectés
        /// </summary>
        /// <param name="input">Objet de trasnfert contenant les données</param>
        public void SendDataToAllNodes(DataInput input)
        {
            DataFormater.Serialize(input);
            /* Multi Client */
            foreach (var node in Nodes)
            {
                SendData(node.Value, input);
            }
        }

        /// <summary>
        /// Méthode appelant l'envoi de résultats à tous les clients connectés
        /// </summary>
        /// <param name="input">Objet de trasnfert contenant les résultats</param>
        private void SendDataToAllClients(DataInput input)
        {
            DataFormater.Serialize(input);
            /* Multi Client */
            foreach (var node in Clients)
            {
                SendData(node.Value, input);
            }
        }

        /// <summary>
        /// Méthode d'exécution d'un TaskExecutor avec la méthode reçue dans l'objet de transfert
        /// </summary>
        /// <param name="input">Objet de transfert contenant la méthode</param>
        /// <param name="node">Noeud emetteur</param>
        public override void ProcessInput(DataInput input, Node node)
        {
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
        /// <summary>
        /// Méthode d'identification d'une connexioon entrante d'un noeud
        /// </summary>
        /// <param name="data">Objet de transfert contenant les informations du noeud emetteur</param>
        /// <returns></returns>
        public object IdentNode(DataInput data)
        {
            foreach (Tuple<int, Node> node in unidentifiedNodes)
            {
                if (node.Item1 == data.TaskId)
                {
                    if (data.ClientGuid != null)
                    {
                        node.Item2.NodeGuid = data.ClientGuid;
                        Clients.TryAdd(data.ClientGuid, node.Item2);
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
                        Nodes.TryAdd(data.NodeGuid, node.Item2);
                        SendNodeToClients(node.Item2);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Méthode déclencheant l'envoi d'informations de monitoring au client
        /// </summary>
        /// <param name="client">Client cible</param>
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

        /// <summary>
        /// Méthode envoyant _une requête de monitoring à un noeud
        /// </summary>
        /// <param name="d">Objet de trasnfert contenant la requête</param>
        /// <param name="n">Noeud cible</param>
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

        /// <summary>
        /// Méthode de réception des requêtes de monitoring émises par un client
        /// </summary>
        /// <param name="input">objet de transfert contenant la requête</param>
        /// <returns></returns>
        private object ProcessCpuStateOrder(DataInput input)
        {
            if (input.MsgType == MessageType.Call)
            {
                if (monitorTask == null)
                {
                    int newTaskId = LastTaskId;
                    monitorTask = new Tuple<int, NodeState>(newTaskId, NodeState.Work);
                }
                GetClientFromGuid(input.ClientGuid).Tasks.TryAdd(monitorTask.Item1, new Task(monitorTask.Item1, NodeState.Work, "MONITOR_TASK"));
                input.NodeGuid = NodeGuid;
                input.TaskId = monitorTask.Item1;
                SendDataToAllNodes(input);
            }
            else if (input.MsgType == MessageType.Response && monitorTask != null)
            {
                foreach (KeyValuePair<string, Node> client in Clients)
                {
                    if (client.Value.Tasks.TryGetValue(monitorTask.Item1, out Task _))
                    {
                        SendData(client.Value, input);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Méthode de remontée d'informations au client
        /// </summary>
        /// <param name="input">Objet de transfert</param>
        /// <returns></returns>
        private object RefreshTaskState(DataInput input)
        {
            // On fait transiter l'info au client
            SendData(GetClientFromGuid(input.ClientGuid), input);
            return null;
        }

        #endregion

        #region Map Reduce
        /// <summary>
        /// Méthode de gestion des traitements envoyés et reçus avec MapReduce
        /// </summary>
        /// <param name="input">Objet de transfert</param>
        /// <returns></returns>
        protected object ProcessMapReduce(DataInput input)
        {
            Logger.Write($"Traitement en cours pour la méthode {input.Method}");
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
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

        /// <summary>
        /// Méthode permettant d'appeler le Reducer sur une liste de résultats par le biais d'un TaskExecutor
        /// </summary>
        /// <param name="input">Objet de transfert contenant un résultat</param>
        /// <param name="executor">TaskExecutor chargé d'exécuter le Reducer</param>
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
                    Logger.Write(@"Réduction des résultats des nodes.");
                    object reduceRes = executor.Reducer.Reduce(result);
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
                    Logger.Write(@"Envoi de la progression au client.");
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
                Logger.Write($"Erreur : Abandon du traitement de la tâche {input.Method} avec l'id {input.TaskId}");
                if (!IsTaskReceiveAllRes(input.TaskId)) return;
                RemoveTask(input.TaskId);
                RemoveResultForTask(input.TaskId);
            }
        }


        /// <summary>
        /// Méthode chargée de transmettre un traitement à un mapper par le biais d'un TaskExecutor 
        /// </summary>
        /// <param name="input">Objet de transfert contenant la donnée</param>
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
                    foreach (KeyValuePair<string, Node> node in Nodes)
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
                            Logger.Write($"Envoi d'une partie du traitement vers le node {node.Value.NodeGuid}");
                            if (mapper.MapIsEnd())
                            {
                                Logger.Write($"Mapping terminé pour la tâche {input.Method} avec l'id {newTaskId}");
                                // endMap vaudra true si on a déjà tout mapper
                                endMap = true;
                                // Si on a tout mapper on l'indique dans le tableau de distribution des NodeTask
                                SetTaskIsMapped(newTaskId);
                            }

                            allNodeWork = false;
                        }
                    }
                    if (allNodeWork)
                    {
                        busy.Reset();
                    }
                }
            };

            Nodes.CollectionChanged += delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                // Si il est en train d'attendre on relance le map
                bool unlockThread = false;
                if (e.NewItems != null)
                {
                    foreach (object node in e.NewItems)
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

        /// <summary>
        /// Méthode de mise à jour du statut d'un noeud au client
        /// </summary>
        /// <param name="node">Noeud contenant le statut désiré</param>
        /// <param name="newTaskId">Id de la tâche</param>
        /// <param name="newNodeTaskId">Id de la tâche gérée par le noeud</param>
        /// <param name="input">objet de transfert contenant les informations du noeud</param>
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
        /// <summary>
        /// Méthode mettant à jour le statut d'une tâche si elle est mappée
        /// </summary>
        /// <param name="newTaskId">ID de la tâche mappée </param>
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
                throw new Exception("Aucune Task avec cet id.");
            }
        }

        /// <summary>
        /// Méthode permettant de découper une tâche demandée par le client pour les différents noeuds recepteur
        /// </summary>
        /// <param name="node">Noeud emetteur</param>
        /// <param name="newTaskId">Id de la tâche à dédcouper</param>
        /// <param name="input">Objet de transfert</param>
        /// <param name="data">Données à associer à cette tâche</param>
        /// <returns></returns>
        private int SendSubTaskToNode(Node node, int newTaskId, DataInput input, object data)
        {
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

        /// <summary>
        /// Méthode permettant d'associer une tâche reçue à un client
        /// </summary>
        /// <param name="input">Objet de transfert contenant la méthode à associer à la tâche</param>
        /// <param name="newTaskId">ID de la tâche</param>
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

        /// <summary>
        /// Méthode permettant d'associer une tâche reçue à un noeud
        /// </summary>
        /// <param name="node">Noeud cible</param>
        /// <param name="newTaskId">ID de la tâche à associer aux noeuds</param>
        /// <param name="newSubTaskId">Id de la sous-tâche à associer à chaque noeud</param>
        private void CreateNodeTasks(Node node, int newTaskId, int newSubTaskId)
        {
            // On ajoute la subtask au node 
            node.Tasks.TryAdd(newSubTaskId, new Task(newSubTaskId, NodeState.Wait));
            // Ajout de la node task à la ligne de la task dans le tableau de distribution des nodetask

            if (TaskDistrib.TryGetValue(newTaskId, out Tuple<ConcurrentBag<int>, bool> task))
            {
                task.Item1.Add(newSubTaskId);
            }
            else
            {
                throw new Exception("Aucune Task avec cet id ");
            }
        }


        /// <summary>
        /// Récupère le pourcentage de progression d'une tpache
        /// </summary>
        /// <param name="taskDist">Liste des ID des résultats terminés ou non</param>
        /// <returns>valeur de pourcentage</returns>
        private double GetProgressionForTask(Tuple<ConcurrentBag<int>, bool> taskDist)
        {
            int nbNodeWork = taskDist.Item1.Count();
            int nbNodeEnded = 0;
            foreach (KeyValuePair<string, Node> node in Nodes)
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
        /// <summary>
        /// Met à jour l'état d'une tâche 
        /// </summary>
        /// <param name="id">ID de la tâche</param>
        /// <param name="status">Etat</param>
        /// <returns>Tâche mise à jour</returns>
        private Task UpdateTaskStatus(int id, NodeState status)
        {
            foreach (KeyValuePair<string, Node> client in Clients)
            {
                if (client.Value.Tasks.TryGetValue(id, out Task task))
                {
                    Task newTask = new Task(id, status) { StartTime = task.StartTime };
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

        /// <summary>
        /// Met à jour l'état d'une tâche attribuée à une noeud
        /// </summary>
        /// <param name="nodeTaskId">Id de la tâche du npoeud</param>
        /// <param name="status">Etat</param>
        /// <param name="nodeGuid">Id du noeud</param>
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

        /// <summary>
        /// Met à jour l'état d'un noeud
        /// </summary>
        /// <param name="status">Etat</param>
        /// <param name="nodeGuid">ID du noeud</param>
        private void UpdateNodeStatus(NodeState status, string nodeGuid)
        {
            if (Nodes.TryGetValue(nodeGuid, out Node node))
            {
                node.State = status;
            }
            else
            {
                throw new Exception("Aucun Node trouvé avec ce GUID");
            }
        }

        /// <summary>
        /// Vérifie si une tâche attribuée à un noeud n'est pas en erreur ou arrêtée
        /// </summary>
        /// <param name="taskId">ID d'une tâche</param>
        /// <returns></returns>
        private bool TaskIsOk(int taskId)
        {
            Node client = GetClientFromTaskId(taskId);
            return GetTaskState(taskId) != NodeState.Error && client.State != NodeState.Dead;
        }

        #endregion

        #region Utilitary methods
        /// <summary>
        /// Identifie un noeud connecté auprès du client
        /// </summary>
        /// <param name="n">Noeud</param>
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

        /// <summary>
        /// Récupère un noeud par son ID
        /// </summary>
        /// <param name="guid">ID</param>
        /// <returns>Noeud associé</returns>
        protected Node GetNodeFromGuid(string guid)
        {
            return Nodes.TryGetValue(guid, out Node node) ? node : null;
        }

        /// <summary>
        /// récupère un client par son ID
        /// </summary>
        /// <param name="guid">ID</param>
        /// <returns>CLient associé</returns>
        protected Node GetClientFromGuid(string guid)
        {
            return (from client in Clients where client.Value.NodeGuid.Equals(guid) select client.Value).FirstOrDefault();
        }

        /// <summary>
        /// Envoie les informations des noeuds à un client
        /// </summary>
        /// <param name="client">Client cible</param>
        public void SendNodesToClient(Node client)
        {
            //TODO check Enumerator Thread safe
            List<List<string>> monitoringValues = new List<List<string>>();
            using (IEnumerator<KeyValuePair<string, Node>> en = Nodes.GetEnumerator())
            {
                while (en.MoveNext())
                {
                    KeyValuePair<string, Node> node = en.Current;
                    List<string> l = GetMonitoringInfos(node.Value);
                    if (l != null)
                    {
                        monitoringValues.Add(l);
                    }
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
        /// <summary>
        /// Récupère l'état d'une tâche
        /// </summary>
        /// <param name="taskId">ID de la tâche</param>
        /// <returns>Etat</returns>
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

        /// <summary>
        /// Récupère l'état d'une sous-tâche
        /// </summary>
        /// <param name="taskId">ID de la sous-tâche</param>
        /// <returns>Etat</returns>
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

        /// <summary>
        /// Vérifie si tous les noeuds associés à une tâche sont en état FINISH
        /// </summary>
        /// <param name="taskId">ID de la tâche</param>
        /// <returns>oui ou non</returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        private bool TaskIsCompleted(int taskId)
        {
            bool completed = true;

            if (TaskDistrib.TryGetValue(taskId, out Tuple<ConcurrentBag<int>, bool> task))
            {
                // Si le booleen vaut true c'est que le mapping est terminé 
                // donc on peut vérifier si toutes les subtask ont été process
                if (task.Item2)
                {
                    // On itère sur toute la liste des subtask de cette task
                    foreach (int subTask in task.Item1)
                    {
                        NodeState state = GetSubTaskState(subTask);
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
            return completed;
        }

        /// <summary>
        /// Supprime les résultats traités d'une tâche
        /// </summary>
        /// <param name="taskId">Id de la tâche</param>
        private void RemoveResultForTask(int taskId)
        {
            if (!Results.TryRemove(taskId, out ConcurrentBag<object> _))
            {
                throw new Exception("Aucune Result avec ce task id");
            }
        }
        /// <summary>
        /// Supprime un noeud déconnecté
        /// </summary>
        /// <param name="node">Noeud</param>
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
                if (!Clients.TryRemove(node.NodeGuid, out var _))
                {
                    Logger.Write("Impossible de supprimer le client de la liste des clients.");
                }
                else
                {
                    Logger.Write("Client supprimé de la liste des clients après déconnexion.");
                }
            }

        }

        /// <summary>
        /// Informe qu'une tâche associée à un noeud est en état d'erreur
        /// </summary>
        /// <param name="node">Noeud</param>
        /// <param name="isClient">si le noeud était renseigné à un client</param>
        /// <returns></returns>
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
                if (id == monitorTask.Item1) continue;
                SetTaskIsMapped(id);
                errorTasks.Add(UpdateTaskStatus(id, NodeState.Error));
            }
            return errorTasks;
        }

        /// <summary>
        /// Retourne toutes les ID des tâches actuelles
        /// </summary>
        /// <param name="taskIds">Liste des ID des tâches</param>
        /// <returns>Liste d'ID</returns>
        private List<int> GetAllNodeTaskIdForClient(List<int> taskIds)
        {
            List<int> nodeTaskId = new List<int>();
            foreach(int id in taskIds)
            {
                foreach (KeyValuePair<int, Tuple<ConcurrentBag<int>, bool>> taskDist in TaskDistrib)
                {
                    if(taskDist.Key == id)
                    {
                        nodeTaskId.AddRange(taskDist.Value.Item1);
                    }
                }
            }
            
            return nodeTaskId;
        }

        /// <summary>
        /// Récupère les ID des tâches émise par le client depuis les noeuds
        /// </summary>
        /// <param name="node">Noeud</param>
        /// <returns>Liste d'ID</returns>
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

        /// <summary>
        /// Récupère un client depuis une ID de tâche
        /// </summary>
        /// <param name="taskId">ID d'une tâche</param>
        /// <returns>Client</returns>
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

        /// <summary>
        /// Supprime une tâche depuis son ID
        /// </summary>
        /// <param name="taskId">ID de la tâche</param>
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

        /// <summary>
        /// Vérifie si une tâche a récupéré tous ses résultats
        /// </summary>
        /// <param name="taskId">ID de la tâche</param>
        /// <returns>oui ou non</returns>
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