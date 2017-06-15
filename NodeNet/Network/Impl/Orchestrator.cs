using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Net;
using NodeNet.Network.Iface;

namespace NodeNet.Network.Impl
{
    public abstract class Orchestrator: IOrchestrator
    {
        public String Address { get; set; }
        public int Port { get; set; }
        public String Name { get; set; }
        private List<INode> Nodes { get; set; }
        private INode self;
    
        private Socket socketServer;

        public Orchestrator(string name, string address,int port, INode self)
        {
            Nodes = new List<INode>();
        }

        public Orchestrator(string name, string address, int port)
        {
            this.Name = name;
            this.Address = address;
            this.Port = port;
        }

        public void Listen()
        {
            DiscoverNodes();
            socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketServer.Bind(new IPEndPoint(IPAddress.Parse(Address), Port));
            socketServer.Listen(10);
            socketServer.BeginAccept(new AsyncCallback(ConnectCallback), socketServer);
        }


        private void ConnectCallback(IAsyncResult asyncResult)
        {
            Socket client = socketServer.EndAccept(asyncResult);
            Console.WriteLine("New Client connected address : " + ((IPEndPoint)client.RemoteEndPoint).Address + " port : " + ((IPEndPoint)client.RemoteEndPoint).Port);
        }


      //  public abstract void mapData(DataInput input);

      //  public abstract void reduceData(DataOutput output);

        public void AddNode(INode node)
        {
            Nodes.Add(node);
            node.registerOrch(this);
        }

        public void DeleteNode(INode node)
        {
            node.stop();
            Nodes.Remove(node);
        }

        public abstract void DiscoverNodes();

        public void Stop()
        {
            foreach(INode node in Nodes)
            {
                DeleteNode(node);
            }
        }

       
    }
}