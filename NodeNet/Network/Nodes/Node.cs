using NodeNet.Data;
using NodeNet.Network.Orch;
using NodeNet.Worker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NodeNet.Network.Nodes
{
    public abstract class Node : INode
    {
        public String NodeGUID;

        public Node Orch { get; set; }

        public String Address { get; set; }
        public int Port { get; set; }
        public String Name { get; set; }

        public Socket NodeSocket { get; set; }
        public Socket ServerSocket { get; set; }

        public static int BUFFER_SIZE = 4096;
        public PerformanceCounter PerfCpu { get; set; }
        public PerformanceCounter PerfRam { get; set; }

        private float cpuValue { get; set; }
        public float CpuValue { get { return (float)(Math.Truncate(cpuValue * 100.0) / 100.0); } set { cpuValue = value; } }
        private double ramValue { get; set; }
        public double RamValue { get { return (Math.Truncate(ramValue * 100.0) / 100.0); } set { ramValue = value; } }

        public GenericWorkerFactory WorkerFactory { get; set; }

        protected List<byte[]> bytearrayList { get; set; }

        protected static ManualResetEvent sendDone = new ManualResetEvent(false);
        protected static ManualResetEvent receiveDone = new ManualResetEvent(false);
        protected static ManualResetEvent connectDone = new ManualResetEvent(false);

        public Node(String name, String adress, int port)
        {
            WorkerFactory = GenericWorkerFactory.GetInstance();
            Name = name;
            Address = adress;
            Port = port;
            NodeGUID = name + " " + Address + " " + Port;
        }

        public Node(string name, string adress, int port, Socket sock)
        {
            Name = name;
            Address = adress;
            Port = port;
            NodeSocket = sock;
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
            Orch = new DefaultNode("Orch", address, port, ServerSocket);
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
                Receive(Orch);

            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SendData(Node node, DataInput obj)
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
                    Object result = ProcessInput(input,node);
                    if (result != null)
                    {
                        DataInput res = new DataInput()
                        {
                            MsgType = MessageType.RESPONSE,
                            Method = input.Method,
                            Data = result,
                            ClientGUID = input.ClientGUID,
                            NodeGUID = this.NodeGUID
                        };
                        receiveDone.Set();
                        SendData(node, res);
                    }
                }             
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());

            }

        }

        public abstract Object ProcessInput(DataInput input,Node node);

        

        public void StartMonitoring()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            bw.DoWork += (o, a) =>
            {
                ManagementObjectSearcher wmiObject = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                if (PerfCpu == null)
                {
                    PerfCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                }
                while (true)
                {
                    dynamic worker = WorkerFactory.GetWorker<Object, Object>("GET_CPU");

                    object result = worker.NodeWork(new Tuple<PerformanceCounter, ManagementObjectSearcher>(PerfCpu, wmiObject));

                    DataInput perfInfo = new DataInput()
                    {
                        MsgType = MessageType.RESPONSE,
                        Method = "GET_CPU",
                        Data = worker.CastInputData(result)
                    };
                    SendData(Orch, perfInfo);
                    Console.WriteLine("Send node info to server");
                    Thread.Sleep(3000);
                }
            };
            bw.RunWorkerAsync();
        }
    }
}
