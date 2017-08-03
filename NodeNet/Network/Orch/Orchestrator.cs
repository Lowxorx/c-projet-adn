using NodeNet.Data;
using NodeNet.Network.Nodes;
using NodeNet.Tasks.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace NodeNet.Network.Orch
{
    public abstract class Orchestrator : Node, IOrchestrator
    {
        /* Bientôt useless */
        private List<Tuple<int, Node>> UnidentifiedNodes;
        /* Correspondance entre les subTasks et les task */
        private List<Tuple<int, List<Tuple<int, State>>>> Tasks;
        /* Stockage des résultats réduits par Task */
        private List<Tuple<int, Object>> Results;
        /* Liste des noeuds connectés */
        private List<Tuple<List<int>, Node>> Nodes;
        /* Liste des clients connectés */
        private List<Tuple<List<int>, Node>> Clients;
        // Task monitoring nodes
        Tuple<int, State> MonitorTask;


        public Orchestrator(string name, string address, int port) : base(name, address, port)
        {
            UnidentifiedNodes = new List<Tuple<int, Node>>();
            Nodes = new List<Tuple<List<int>, Node>>();
            Clients = new List<Tuple<List<int>, Node>>();
            Tasks = new List<Tuple<int, List<Tuple<int, State>>>>();
            WorkerFactory.AddWorker("IDENT", new IdentificationTask(IdentNode));
            WorkerFactory.AddWorker("GET_CPU", new CPUStateTask(ProcessCPUStateOrder));
        }

        public async void Listen()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(Address), Port);
            listener.Start();
            Console.WriteLine("Server is listening on port : " + Port);
            while (true)
            {
                Socket sock = await listener.AcceptSocketAsync();
                DefaultNode connectedNode = new DefaultNode("Node ", ((IPEndPoint)sock.RemoteEndPoint).Address + "", ((IPEndPoint)sock.RemoteEndPoint).Port, sock);
                /* Multi Client */
                Console.WriteLine(String.Format("Client Connection accepted from {0}", sock.RemoteEndPoint.ToString()));
                GetIdentityOfNode(connectedNode);
                Receive(connectedNode);
                /* Multi Client */
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
                TaskId = tId
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
            foreach (Tuple<List<int>, Node> tuple in Nodes)
            {
                try
                {
                    //Console.WriteLine("Send data : " + input + " to : " + tuple.Item2);
                    //tuple.Item2.NodeSocket.BeginSend(data, 0, data.Length, 0,
                    //    new AsyncCallback(SendCallback), tuple.Item2.NodeSocket);
                    SendData(tuple.Item2, input);
                }
                catch (SocketException ex)
                {
                    /// Client Down ///
                    if (!tuple.Item2.NodeSocket.Connected)
                    {
                        Console.WriteLine("Client " + tuple.Item2.NodeSocket.RemoteEndPoint.ToString() + " Disconnected");
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
            if (input.Method == "GET_CPU")
            {
                dynamic worker = WorkerFactory.GetWorker<Object, Object>(input.Method);
                worker.OrchWork(input);
            }
            else if (input.Method == "IDENT")
            {
                input.Data = node;
                IdentNode(input);
            }
            else
            {
                dynamic worker = WorkerFactory.GetWorker<Object, Object>(input.Method);
                worker.OrchWork(input);
            }

            return null;
        }

        /* Multi Client */
        public void IdentNode(DataInput data)
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
                        Clients.Add(new Tuple<List<int>, Node>(new List<int>(), node.Item2));
                        SendNodesToClient(node.Item2);
                    }
                    else if (data.NodeGUID != null)
                    {
                        node.Item2.NodeGUID = data.NodeGUID;
                        if (MonitorTask != null)
                        {
                            StartMonitoringForNode(data, node.Item2);
                        }
                        Console.WriteLine("Add Node to list : " + node);
                        Nodes.Add(new Tuple<List<int>, Node>(new List<int>(), node.Item2));
                        SendNodeToClients(node.Item2);
                    }
                }
            }
        }

        public void SendNodesToClient(Node n)
        {
            List<List<String>> monitoringValues = new List<List<String>>();

            foreach (Tuple<List<int>, Node> tuple in Nodes)
            {
                List<string> l = GetMonitoringInfos(tuple.Item2);
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
                    Data = monitoringValues
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
            foreach (Tuple<List<int>, Node> tuple in Clients)
            {
                DataInput di = new DataInput()
                {
                    ClientGUID = tuple.Item2.NodeGUID,
                    NodeGUID = NodeGUID,
                    Method = "IDENT",
                    Data = monitoringValues
                };
                SendData(tuple.Item2, di);

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

        protected Tuple<Boolean, Node> GetNodeFromGUID(String guid)
        {
            foreach (Tuple<List<int>, Node> tuple in Clients)
            {
                if (tuple.Item1.Equals(guid))
                {
                    return new Tuple<bool, Node>(true, tuple.Item2);
                }
            }
            foreach (Tuple<List<int>, Node> tuple in Nodes)
            {
                if (tuple.Item1.Equals(guid))
                {
                    return new Tuple<bool, Node>(false, tuple.Item2);
                }
            }
            throw new Exception();
        }

        protected Tuple<List<int>, Node> GetClientTaskFromGuid(String guid)
        {
            foreach (Tuple<List<int>, Node> tuple in Clients)
            {
                if (tuple.Item2.NodeGUID.Equals(guid))
                {
                    return tuple;
                }
            }
            throw new Exception();
        }

        protected Node GetClientFromGUID(String guid)
        {
            foreach (Tuple<List<int>, Node> tuple in Clients)
            {
                if (tuple.Item2.NodeGUID.Equals(guid))
                {
                    return tuple.Item2;
                }
            }
            throw new Exception("GetClientFromGuid");
        }

        private void ProcessCPUStateOrder(DataInput input)
        {
            if (input.MsgType == MessageType.CALL)
            {
                if (MonitorTask == null)
                {
                    int newTaskID = LastTaskID;
                    MonitorTask = new Tuple<int, State>(newTaskID, State.WORK);
                }
                GetClientTaskFromGuid(input.ClientGUID).Item1.Add(MonitorTask.Item1);
                input.NodeGUID = NodeGUID;
                input.TaskId = MonitorTask.Item1;
                SendDataToAllNodes(input);
            }
            else if (input.MsgType == MessageType.RESPONSE && MonitorTask != null)
            {
                foreach (Tuple<List<int>, Node> tuple in Clients)
                {
                    if (tuple.Item1.Contains(MonitorTask.Item1))
                    {
                        SendData(tuple.Item2, input);
                    }
                }
            }
        }

        protected void ProcessMapReduce(DataInput input)
        {
            dynamic worker = WorkerFactory.GetWorker<Object, Object>(input.Method);
            Console.WriteLine("Process Display Function on Orch");
            if (input.MsgType == MessageType.CALL)
            {
                // MAP
                List<Object> list = worker.Mapper.map(worker.CastDataInput(input.Data));
                LazyNodeTranfert(list, input);
            }
            else if (input.MsgType == MessageType.RESPONSE)
            {
                // Reduce
                // On cherche l'emplacement du resultat pour cette task et on l'envoit au Reduce 
                // pour y concaténeer le resultat du travail du noeud
                Tuple<int, Object> result = null;
                foreach (Tuple<int, Object> tuple in Results)
                {
                    if (tuple.Item1 == input.TaskId)
                    {
                        result = tuple;
                    }
                }
                Object reduceRes = worker.Reducer.reduce(worker.CastDataInput(result.Item2), worker.CastDataInput(input.Data));
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
                    SendData(GetClientFromGUID(input.ClientGUID), response);
                }
                else
                {
                    result = new Tuple<int, Object>(input.TaskId, reduceRes);
                }

            }
        }
        // Checker si toutes les nodes correspondant à cette task sont en etat FINISH
        private bool TaskIsCompleted(int taskId)
        {
            // STUB
            return true;
        }

        private void LazyNodeTranfert(List<Object> data, DataInput input)
        {
            int newTaskID = LastTaskID;
            Tuple<int, List<Tuple<int, State>>> newTask = new Tuple<int, List<Tuple<int, State>>>(newTaskID, new List<Tuple<int, State>>());
            Tuple<int, Object> emptyResult = new Tuple<int, Object>(newTaskID, null);
            Results.Add(emptyResult);
            for (int i = 0; i < data.Count; i++)
            {
                Tuple<int, State> newSubTask = new Tuple<int, State>(LastSubTaskID, State.WORK);
                newTask.Item2.Add(newSubTask);
                UpdateNodeAndClientTasks(input.ClientGUID, input.NodeGUID, newTaskID, LastSubTaskID);
                DataInput res = new DataInput()
                {
                    TaskId = newTaskID,
                    SubTaskId = LastSubTaskID,
                    MsgType = MessageType.CALL,
                    Method = input.Method,
                    Data = data[i],
                    ClientGUID = input.ClientGUID,
                    NodeGUID = this.NodeGUID,
                };
                Node node = Nodes[Nodes.Count % i].Item2;
                SendData(node, res);
            }
        }

        private void UpdateNodeAndClientTasks(string clientGUID, string nodeGUID, int newTaskID, int newSubTaskID)
        {
            foreach (Tuple<List<int>, Node> tuple in Clients)
            {
                if (tuple.Item2.NodeGUID.Equals(clientGUID))
                {
                    tuple.Item1.Add(newTaskID);
                }
            }

            foreach (Tuple<List<int>, Node> tuple in Nodes)
            {
                if (tuple.Item2.NodeGUID.Equals(nodeGUID))
                {
                    tuple.Item1.Add(newSubTaskID);
                }
            }

        }
    }
}