using NodeNet.Data;
using NodeNet.Network.Nodes;
using NodeNet.Network.States;
using NodeNet.Tasks;
using System;
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
        /* Bientôt useless */
        private List<Tuple<int, Node>> UnidentifiedNodes;
        /* Nombre de noeuds connectés */
        private int nbNodes = 0;
       
              
        // Task monitoring nodes */
        Tuple<int, NodeState> MonitorTask;

        /* Stockage des résultats réduits par Task */
        private List<Tuple<int, Object>> results;        
        public List<Tuple<int, Object>> Results
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return results; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set { results = value; }
        }


        /* Liste des noeuds connectés */
        private ObservableCollection<Node> nodes;
        public ObservableCollection<Node> Nodes
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return nodes; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set { nodes = value; }
        }

        /* Liste des clients connectés */
        private List<Node> clients;
        public List<Node> Clients
        {
            get { return clients; }
            set { clients = value; }
        }


        public Orchestrator(string name, string address, int port) : base(name, address, port)
        {
            UnidentifiedNodes = new List<Tuple<int, Node>>();
            Nodes = new ObservableCollection<Node>();
            Clients = new List<Node>();
            WorkerFactory.AddWorker("IDENT", new TaskExecutor(this,IdentNode,null,null));
            WorkerFactory.AddWorker("GET_CPU", new TaskExecutor(this,ProcessCPUStateOrder,null,null));
        }

        public async void Listen()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(Address), Port);
            listener.Start();
            Console.WriteLine("Server is listening on port : " + Port);
            while (true)
            {
                Socket sock = await listener.AcceptSocketAsync();
                DefaultNode connectedNode = new DefaultNode("", ((IPEndPoint)sock.RemoteEndPoint).Address + "", ((IPEndPoint)sock.RemoteEndPoint).Port, sock);
                nbNodes++;
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
                Method = "IDENT",
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
            foreach (Node node in Nodes)
            {
                try
                {
                    SendData(node, input);
                }
                catch (SocketException ex)
                {
                    /// Client Down ///
                    if (!node.NodeSocket.Connected)
                    {
                        Console.WriteLine("Client " + node.NodeSocket.RemoteEndPoint.ToString() + " Disconnected");
                    }
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public override void ReceiveCallback(IAsyncResult ar)
        {
            Tuple<Node, byte[]> state = (Tuple<Node, byte[]>)ar.AsyncState;
            byte[] buffer = state.Item2;
            Node node = state.Item1;
            Socket client = node.NodeSocket;
            try
            {
                // Read data from the remote device.
                int bytesRead = client.EndReceive(ar);
                Console.WriteLine("Number of bytes received : " + bytesRead);
                bytearrayList = new List<byte[]>();
                if (bytesRead == 4096)
                {
                    byte[] data = buffer;
                    bytearrayList.Add(data);
                }
                else
                {
                    DataInput input;
                    if (bytearrayList.Count > 0)
                    {
                        byte[] data = bytearrayList
                                     .SelectMany(a => a)
                                     .ToArray();
                        input = DataFormater.Deserialize<DataInput>(data);
                    }
                    else
                    {
                        input = DataFormater.Deserialize<DataInput>(buffer);
                    }


                    ProcessInput(input, node);
                    receiveDone.Set();
                }
                Receive(node);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public override object ProcessInput(DataInput input, Node node)
        {
            if (!input.Method.Equals("IDENT") && !input.Method.Equals("GET_CPU"))
            {
                ProcessMapReduce(input);
            }
            else
            {
                TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
                return executor.DoWork(input);
            }
            return null;
        }

        /* Multi Client */
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
                        Clients.Add(node.Item2);
                        // TODO activer le monitoring pour ce client
                        SendNodesToClient(node.Item2);
                        startMonitoringForClient(node.Item2);
                    }
                    else if (data.NodeGUID != null)
                    {
                        node.Item2.NodeGUID = data.NodeGUID;
                        if (MonitorTask != null)
                        {
                            StartMonitoringForNode(data, node.Item2);
                        }
                        Console.WriteLine("Add Node to list : " + node);
                        Nodes.Add(node.Item2);
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
                Method = "GET_CPU",
                Data = null,
                ClientGUID = client.NodeGUID,
                NodeGUID = NodeGUID,
                MsgType = MessageType.CALL
            };
            ProcessCPUStateOrder(input);
        }

        public void SendNodesToClient(Node n)
        {
            List<List<String>> monitoringValues = new List<List<String>>();

            foreach (Node node in Nodes)
            {
                List<string> l = GetMonitoringInfos(node);
                if (l != null)
                {
                    monitoringValues.Add(l);
                }
            }
            if (monitoringValues.Count > 0)
            {
                DataInput di = new DataInput()
                {
                    ClientGUID = n.NodeGUID,
                    NodeGUID = NodeGUID,
                    Method = "IDENT",
                    Data = monitoringValues,
                    MsgType = MessageType.NODE_IDENT
                };
                SendData(n, di);
            }
        }

        private void SendNodeToClients(Node n)
        {
            List<List<String>> monitoringValues = new List<List<String>>
            {
                GetMonitoringInfos(n)
            };
            foreach ( Node node in Clients)
            {
                DataInput di = new DataInput()
                {
                    ClientGUID = node.NodeGUID,
                    NodeGUID = NodeGUID,
                    Method = "IDENT",
                    Data = monitoringValues,
                    MsgType = MessageType.NODE_IDENT
                };
                SendData(node, di);

            }
        }

        public void StartMonitoringForNode(DataInput d, Node n)
        {
            DataInput input = new DataInput()
            {
                Method = "GET_CPU",
                Data = null,
                ClientGUID = d.ClientGUID,
                NodeGUID = NodeGUID,
                MsgType = MessageType.CALL,
                TaskId = MonitorTask.Item1
            };
            SendData(n, input);
        }

        protected Node GetNodeFromGUID(String guid)
        {
            foreach (Node client in Clients)
            {
                if (client.NodeGUID.Equals(guid))
                {
                    return client;
                }
            }
            foreach (Node node in Nodes)
            {
                if (node.NodeGUID.Equals(guid))
                {
                    return node;
                }
            }
            throw new Exception();
        }

        protected Node GetClientFromGUID(String guid)
        {
            foreach (Node node in Clients)
            {
                if (node.NodeGUID.Equals(guid))
                {
                    return node;
                }
            }
            throw new Exception("GetClientFromGuid");
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
                GetClientFromGUID(input.ClientGUID).Tasks.Add(new Tuple<int, List<int>>(MonitorTask.Item1, new List<int>()));
                input.NodeGUID = NodeGUID;
                input.TaskId = MonitorTask.Item1;
                SendDataToAllNodes(input);
            }
            else if (input.MsgType == MessageType.RESPONSE && MonitorTask != null)
            {
                foreach ( Node client in Clients)
                {
                    foreach(Tuple<int, List<int>> task in client.Tasks)
                    {
                        if(task.Item1 == MonitorTask.Item1)
                        {
                            SendData(client, input);
                        }
                    }
                }
            }
            return null;
        }

        protected Object ProcessMapReduce(DataInput input)
        {
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
            Console.WriteLine("Process Display Function on Orch");
            if (input.MsgType == MessageType.CALL)
            {
                // MAP
                
                LazyNodeTranfert(input);
            }
            else if (input.MsgType == MessageType.RESPONSE)
            {
                // Reduce
                // On cherche l'emplacement du resultat pour cette task et on l'envoit au Reduce 
                // pour y concaténer le resultat du travail du noeud
                Tuple<int, Object> result = null;
                foreach (Tuple<int, Object> tuple in Results)
                {
                    if (tuple.Item1 == input.TaskId)
                    {
                        result = tuple;
                    }
                }
                Object reduceRes = executor.Reducer.reduce(result.Item2, input.Data);
                if (TaskIsCompleted(input.TaskId))
                {
                    // TODO check si tous les nodes ont finis
                    DataInput response = new DataInput()
                    {
                        TaskId = input.TaskId,
                        Method = input.Method,
                        Data = reduceRes,
                        ClientGUID = input.ClientGUID,
                        NodeGUID = this.NodeGUID,
                        MsgType = MessageType.RESPONSE,
                    };
                    return response;
                    //SendData(GetClientFromGUID(input.ClientGUID), response);
                }
                else
                {
                    result = new Tuple<int, Object>(input.TaskId, reduceRes);
                }     
            }
            return null;
        }
        // Checker si toutes les nodes correspondant à cette task sont en etat FINISH
        private bool TaskIsCompleted(int taskId)
        {
            // STUB
            return true;
        }

        private void LazyNodeTranfert(DataInput input)
        {
            int newTaskID = LastTaskID;
            Tuple<int, List<Tuple<int, NodeState>>> newTask = new Tuple<int, List<Tuple<int, NodeState>>>(newTaskID, new List<Tuple<int, NodeState>>());
            Tuple<int, Object> emptyResult = new Tuple<int, Object>(newTaskID, null);
            Results.Add(emptyResult);

            System.Threading.ManualResetEvent _busy = new System.Threading.ManualResetEvent(false);
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            bw.DoWork += (o, a) =>
            {
                bool kontinue = true;
                while (kontinue)
                {
                    for (int i = 0; i < Nodes.Count && kontinue; i++)
                    {
                        if (Nodes[i].State == NodeState.WAIT)
                            kontinue = sendSubTaskToNode(newTask, Nodes[i], newTaskID, input);
                    }
                    if (kontinue)
                        _busy.Reset();
                }
            };

            Nodes.CollectionChanged += delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                Tuple<List<int>, Node> obj = sender as Tuple<List<int>, Node>;
                if (obj.Item2.State == NodeState.WAIT)
                    _busy.Set();
            };
           
            bw.RunWorkerAsync();
        }
        
        private bool sendSubTaskToNode(Tuple<int, List<Tuple<int, NodeState>>> newTask, Node node, int newTaskID, DataInput input)
        {
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
            Object data = executor.Mapper.map(input.Data);
            if (data != null)
            {
                Tuple<int, NodeState> newSubTask = new Tuple<int, NodeState>(LastSubTaskID, NodeState.WORK);
                newTask.Item2.Add(newSubTask);
                UpdateNodeAndClientTasks(input.ClientGUID, input.NodeGUID, newTaskID, LastSubTaskID);
                DataInput res = new DataInput()
                {
                    TaskId = newTaskID,
                    SubTaskId = LastSubTaskID,
                    MsgType = MessageType.CALL,
                    Method = input.Method,
                    Data = data,
                    ClientGUID = input.ClientGUID,
                    NodeGUID = this.NodeGUID,
                };
                SendData(node, res);
                return true;
            }
            return false;
        }

        private void UpdateNodeAndClientTasks(string clientGUID, string nodeGUID, int newTaskID, int newSubTaskID)
        {
            Node client = GetClientFromGUID(clientGUID);
            bool taskpresent = false;
            foreach (Tuple<int, List<int>> tasks in client.Tasks)
            {
                taskpresent = tasks.Item1 == newTaskID ? true : false;
                if (taskpresent)
                    tasks.Item2.Add(newSubTaskID);
            }
            if (!taskpresent)
                client.Tasks.Add(new Tuple<int, List<int>>(newTaskID, new List<int>(newSubTaskID)));

            Node node = GetNodeFromGUID(nodeGUID);

            bool present = false;
            foreach(Tuple<int, List<int>> tasks in node.Tasks)
            {
                present = tasks.Item1 == newTaskID ? true : false;
                if (present)
                    tasks.Item2.Add(newSubTaskID);
            }
            if(!present)
                node.Tasks.Add(new Tuple<int, List<int>>(newTaskID, new List<int>(newSubTaskID)));

        }
    }
}