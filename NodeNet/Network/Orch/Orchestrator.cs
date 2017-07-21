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
        private List<Node> Nodes { get; set; }
       

        public Orchestrator(string name, string address, int port) : base(name, address, port)
        {
            Nodes = new List<Node>();
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
                Nodes.Add(connectedNode);
                ViewModelLocator.VMLMonitorUcStatic.NodeList.Add(connectedNode);
                Console.WriteLine(String.Format("Client Connection accepted from {0}", sock.RemoteEndPoint.ToString()));
                Receive(connectedNode);
            }
 
        }

        public new void Stop()
        {
            throw new NotImplementedException();
        }

        public void SendDataToAllNodes(DataInput input)
        {
            byte[] data = DataFormater.Serialize(input);

            foreach (Node node in Nodes)
            {
                try
                {
                    node.NodeSocket.BeginSend(data, 0, data.Length, 0,
                        new AsyncCallback(SendCallback), node.NodeSocket);
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
                    this.bytearrayList.Add(data);

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

                    ProcessInput(input);
                    receiveDone.Set();
                }

                Receive(node);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public override object ProcessInput(DataInput input)
        {
            if (input.Method == "GET_CPU")
            {
                dynamic worker = WorkerFactory.GetWorker<Object, Object>(input.Method);
                worker.ProcessResponse(worker.ProcessResponse(worker.CastOrchData(input.Data)));
            }
            return null;
        }
    }
}