using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Net;
using NodeNet.Network.Iface;

namespace NodeNet.Network.Impl
{
    abstract class Orchectrator: IOrchestrator
    {
        public String Address { get; set; }
        public int Port { get; set; }
        public String Name { get; set; }
        private List<INode> nodes { get; set; }
        private INode self;
    
        private Socket socketServer;

        public Orchectrator(string name, string address,int port, INode self)
        {
            nodes = new List<INode>();
        }

        public Orchectrator(string name, string address, int port)
        {
            this.Name = name;
            this.Address = address;
            this.Port = port;
        }

        public void listen()
        {
            discoverNodes();
            socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketServer.Bind(new IPEndPoint(IPAddress.Parse(Address), Port));
            socketServer.Listen(10);
            socketServer.BeginAccept(new AsyncCallback(connectCallback), socketServer);
        }


        private void connectCallback(IAsyncResult asyncResult)
        {
            Socket client = socketServer.EndAccept(asyncResult);
            Console.WriteLine("New Client connected address : " + ((IPEndPoint)client.RemoteEndPoint).Address + " port : " + ((IPEndPoint)client.RemoteEndPoint).Port);
        }


        public abstract void mapData(DataInput input);

        public abstract void reduceData(DataOutput output);

        public void addNode(INode node)
        {
            nodes.Add(node);
            node.registerOrch(this);
        }

        public void deleteNode(INode node)
        {
            node.stop();
            nodes.Remove(node);
        }

        public abstract void discoverNodes();

        public void stop()
        {
            foreach(INode node in nodes)
            {
                deleteNode(node);
            }
        }

       
    }
}