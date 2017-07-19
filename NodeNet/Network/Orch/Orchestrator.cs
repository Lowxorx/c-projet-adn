using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using NodeNet.Network.Nodes;
using NodeNet.Network.Data;
using NodeNet.Worker;
using NodeNet.Worker.Impl;

namespace NodeNet.Network.Orch
{
    public abstract class Orchestrator: Node , IOrchestrator
    {
        private List<INode> Nodes { get; set; }

        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        public Orchestrator(string name, string address,int port) : base(name, address, port)
        {
            Nodes = new List<INode>();
        }

        public async void Listen()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(Address), Port);
            listener.Start();
            Console.WriteLine("Server is listening on port : " + Port);
            while (true)
            {
                Socket sock = await listener.AcceptSocketAsync();
                Node connectedNode = new Node("Node ",((IPEndPoint)sock.RemoteEndPoint).Address+"", ((IPEndPoint)sock.RemoteEndPoint).Port,sock);
                Nodes.Add(connectedNode);
                Console.WriteLine(String.Format("Client Connection accepted from {0}", sock.RemoteEndPoint.ToString()));
                Receive(connectedNode);
            }
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
                        Console.WriteLine("Client " + node.NodeSocket.RemoteEndPoint.ToString() + " Disconnected");
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}