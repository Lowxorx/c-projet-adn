using System;
using System.Net.Sockets;
using System.Net;
using NodeNet.Network.Iface;

namespace NodeNet.Network.Impl
{
    public class Node : INode
    {
        public IOrchestrator orch {get; set;}
        public String address {get; set;}
        public int port {get; set;}
        public String name {get; set;}
        private Socket nodeSocket;

        public Node(String name, String adress, int port)
        {
            this.name = name;
            this.address = address;
            this.port = port;
        }

        public void stop()
        {
            throw new NotImplementedException();
        }

        public void registerOrch(IOrchestrator node)
        {
            orch = node;
        }

        public void connect(string address, int port)
        {
            nodeSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            nodeSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(address), port), new AsyncCallback(connectCallback), nodeSocket);
        }

        private void connectCallback(IAsyncResult asyncResult)
        {   
            Console.WriteLine("Client connected to server");
            nodeSocket.EndConnect(asyncResult);
        }
    }
}
