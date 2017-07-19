using NodeNet.GUI.ViewModel;
using NodeNet.Network.Data;
using NodeNet.Network.Nodes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NodeNet.Network.Orch
{
    public abstract class Orchestrator : Node, IOrchestrator
    {
        private List<Node> Nodes { get; set; }

        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

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
                Node connectedNode = new Node("Node ", ((IPEndPoint)sock.RemoteEndPoint).Address + "", ((IPEndPoint)sock.RemoteEndPoint).Port, sock);
                Nodes.Add(connectedNode);
                ViewModelLocator.VMLMonitorUcStatic.NodeList.Add(connectedNode);
                Console.WriteLine(String.Format("Client Connection accepted from {0}", sock.RemoteEndPoint.ToString()));
                Receive(connectedNode);
            }
            //ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //ServerSocket.Bind(new IPEndPoint(IPAddress.Parse(Address), Port));
            //ServerSocket.Listen(10);
            //ServerSocket.BeginAccept(new AsyncCallback(ConnectCallback), ServerSocket);
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            Socket client = ServerSocket.EndAccept(asyncResult);
            //Console.WriteLine("New Client connected address : " + ((IPEndPoint)client.RemoteEndPoint).Address + " port : " + ((IPEndPoint)client.RemoteEndPoint).Port);
            Receive(this);
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

    }
}