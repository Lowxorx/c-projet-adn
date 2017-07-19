using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using NodeNet.Network.Orch;
using NodeNet.Network.Data;
using NodeNet.Worker;
using NodeNet.Worker.Impl;

namespace NodeNet.Network.Nodes
{
    public class Node : INode
    {
        public Node Orch {get; set;}
        public String Address {get; set;}
        public int Port {get; set;}
        public String Name {get; set;}
        public Socket NodeSocket { get; set; }
        public Socket ServerSocket { get; set; }
        public static int BUFFER_SIZE = 4096;
        public GenericWorkerFactory WorkerFactory { get; set; }
        
        protected static ManualResetEvent sendDone = new ManualResetEvent(false);
        protected static ManualResetEvent receiveDone = new ManualResetEvent(false);
        protected static ManualResetEvent connectDone = new ManualResetEvent(false);

        public Node(String name, String adress, int port)
        {
            WorkerFactory = GenericWorkerFactory.GetInstance();
            try
            {
                WorkerFactory.AddWorker("GET_CPU", new CPUStateWorker());
            }
            catch(Exception e){
                Console.WriteLine(e.Message);
            }
            Name = name;
            Address = adress;
            Port = port;
        }

        public Node(string name, string adress, int port, Socket sock) : this(name, adress, port)
        {
            this.NodeSocket = sock;
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void RegisterOrch(Orchestrator node)
        {
            Orch = node;
        }

        public void Connect(string address, int port)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(address), port);
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Orch = new Node("Orch", address, port, ServerSocket);
            Console.WriteLine("Client started ...");
            try
            {
                ServerSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), ServerSocket);

                connectDone.WaitOne();
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;
                // Complete the connection.
                client.EndConnect(ar);
                Console.WriteLine(String.Format("Connection accepted to {0} ", client.RemoteEndPoint.ToString()));

                // Signal that the connection has been made.
                connectDone.Set();
                while (true)
                {
                    Receive(Orch);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SendData(Node node, object obj)
        {
            byte[] data = DataFormater.Serialize(obj);

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

        public void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to Node.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Receive(Node node)
        {
            try
            {
                // Begin receiving the data from the remote device.
                byte[] buffer = new byte[BUFFER_SIZE];
                node.NodeSocket.BeginReceive(buffer, 0, BUFFER_SIZE, 0,
                    new AsyncCallback(ReceiveCallback), Tuple.Create(node, buffer));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public virtual void ReceiveCallback(IAsyncResult ar)
        {
            //// Retrieve the state object and the client socket 
            //// from the asynchronous state object.
            //Tuple<Node, byte[]> state = (Tuple < Node, byte[] > )ar.AsyncState;
            //byte[] buffer = state.Item2;
            //Node node = state.Item1;
            //Socket client = node.NodeSocket;
            //try
            //{
            //    // Read data from the remote device.
            //    int bytesRead = client.EndReceive(ar);
            //    Console.WriteLine("Number of bytes received : " + bytesRead);
            //    if (bytesRead > 0)
            //    {
            //        DataInput input = DataFormater.Deserialize<DataInput>(buffer);
            //        IWorker worker = WorkerFactory.GetWorker(input.Method);
            //        // Dans le cas d'un noeud client
            //        if (input.msgType == MessageType.CALL)
            //        {
            //            DataInput res = worker.DoWork(input);
            //            if (res != null)
            //            {
            //                res.msgType = MessageType.RESPONSE;
            //                SendData(Orch, res);
            //            }
            //        }
            //        // Dans le cas d'un noeud serveur
            //        else
            //        {
            //            // TODO traiter réponses -> mettre à jour l'état des noeuds sur l'IHM etc...
            //        }
            //    }
            //    else
            //    {
            //        receiveDone.Set();
            //    }
            //}
            //catch (SocketException e)
            //{
            //    Console.WriteLine(e.ToString());

            //}
        }
        public virtual void ProcessReceive(Node node, DataInput input)
        {

        }
    }
}
