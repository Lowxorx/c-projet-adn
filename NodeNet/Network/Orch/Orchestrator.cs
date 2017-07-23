using NodeNet.GUI.ViewModel;
using NodeNet.Data;
using NodeNet.Network.Nodes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using NodeNet.Worker;
using NodeNet.Worker.Impl;

namespace NodeNet.Network.Orch
{
    public abstract class Orchestrator : Node, IOrchestrator
    {
        /* Multi Client */
        private List<Tuple<String, Node>> UnidentifiedNodes { get; set; }
        /* Multi Client */
        private List<Tuple<String, Node>> Nodes { get; set; }
        /* Multi Client */
        private List<Tuple<String, Node>> Clients { get; set; }

        public Orchestrator(string name, string address, int port) : base(name, address, port)
        {
            /* Multi Client */
            UnidentifiedNodes = new List<Tuple<String, Node>>();
            Nodes = new List<Tuple<String, Node>>();
            Clients = new List<Tuple<String, Node>>();
            WorkerFactory.AddWorker("IDENT", new IdentitifierWorker(IdentNode));
            WorkerFactory.AddWorker("GET_CPU", new CPUStateWorker(ProcessCPUStateOrder));
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
                UnidentifiedNodes.Add(new Tuple<string, Node>(connectedNode.Address + " " + connectedNode.Port, connectedNode));
                ViewModelLocator.VMLMonitorUcStatic.NodeList.Add(connectedNode);
                Console.WriteLine(String.Format("Client Connection accepted from {0}", sock.RemoteEndPoint.ToString()));
                Receive(connectedNode);
                /* Multi Client */
                GetIdentityOfNode(connectedNode);
            }

        }

        private void GetIdentityOfNode(DefaultNode connectedNode)
        {
            DataInput input = new DataInput()
            {
                Method = "IDENT",
                NodeGUID = NodeGUID
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
            foreach (Tuple<String, Node> tuple in Nodes)
            {
                try
                {
                    tuple.Item2.NodeSocket.BeginSend(data, 0, data.Length, 0,
                        new AsyncCallback(SendCallback), tuple.Item2.NodeSocket);
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

                    ProcessInput(input,node);
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
            else if (input.Method == "IDENT"){
                input.Data = node;
                IdentNode(input);
            }
            return null;
        }

        /* Multi Client */
        public void IdentNode(DataInput data)
        {
            Console.WriteLine("Process Ident On Orch");
            Node sender = (Node)data.Data;
            // Si Item1 == True alors c'est un client, sinon c'est un simple Node
            foreach (Tuple<String, Node> tuple in UnidentifiedNodes)
            {
                if (sender.NodeGUID == tuple.Item2.NodeGUID)
                {
                    tuple.Item2.NodeGUID = data.NodeGUID;
                    if (data.ClientGUID != null)
                    {
                        Clients.Add(tuple);
                    }
                    else if (data.NodeGUID != null)
                    {
                        Nodes.Add(tuple);
                    }
                    // TODO Check si je peux remove l'item de la liste quand je le parcours
                    UnidentifiedNodes.Remove(tuple);
                    break;
                }
            }  
        }

        protected Tuple<Boolean, Node> GetNodeFromGUID(String guid)
        {
            foreach (Tuple<String, Node> tuple in Clients)
            {
                if (tuple.Item1.Equals(guid))
                {
                    return new Tuple<bool, Node>(true, tuple.Item2);
                }
            }
            foreach (Tuple<String, Node> tuple in Nodes)
            {
                if (tuple.Item1.Equals(guid))
                {
                    return new Tuple<bool, Node>(false, tuple.Item2);
                }
            }
            throw new Exception();
        }

        protected Node GetClientFromGUID(String guid)
        {
            foreach (Tuple<String, Node> tuple in Clients)
            {
                if (tuple.Item1.Equals(guid))
                {
                    return tuple.Item2;
                }
            }
            throw new Exception();
        }

        private void ProcessCPUStateOrder(DataInput obj)
        {
            SendDataToAllNodes(obj);
        }
    }
}