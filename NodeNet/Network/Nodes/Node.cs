using NodeNet.Data;
using NodeNet.Network.Orch;
using NodeNet.Network.States;
using NodeNet.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NodeNet.Network.Nodes
{
    public class StateObject
    {
        // Client socket.
        public Node node = null;
        // Size of receive buffer.
        public const int BufferSize = 4096;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data
        public List<byte[]> data = new List<byte[]>();
    }
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

        public NodeState State { get; set; }

        private List<Task> tasks;

        public List<Task> Tasks
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return tasks; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set { tasks = value; }
        }

        /* Stockage des résultats réduits par Task */
        private List<Tuple<int, List<Object>>> results;
        public List<Tuple<int, List<Object>>> Results
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return results; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set { results = value; }
        }

        private int lastTaskID;

        protected int LastTaskID
        {
            get { return lastTaskID += 1; }
            set { new InvalidOperationException(); }
        }

        private int lastSubTaskID;

        protected int LastSubTaskID
        {
            get { return lastSubTaskID += 1; }
            set { new InvalidOperationException(); }
        }

        private float cpuValue { get; set; }
        public float CpuValue { get { return (float)(Math.Truncate(cpuValue * 100.0) / 100.0); } set { cpuValue = value; } }
        private double ramValue { get; set; }
        public double RamValue { get { return (Math.Truncate(ramValue * 100.0) / 100.0); } set { ramValue = value; } }
        private int workingTask;

        public int WorkingTask
        {
            get { return workingTask; }
            set { workingTask = value; }
        }

        private double progression;

        public double Progression
        {
            get { return progression; }
            set { progression = value; }
        }



        public TaskExecFactory WorkerFactory { get; set; }

        protected List<byte[]> bytearrayList { get; set; }

        protected static ManualResetEvent sendDone = new ManualResetEvent(false);
        protected static ManualResetEvent receiveDone = new ManualResetEvent(false);
        protected static ManualResetEvent connectDone = new ManualResetEvent(false);

        protected const String GET_CPU_METHOD = "GET_CPU";
        protected const String IDENT_METHOD = "IDENT";
        protected const String TASK_STATUS_METHOD = "TASK_STATE";

        public Node(String name, String adress, int port)
        {
            WorkerFactory = TaskExecFactory.GetInstance();
            Name = name;
            Address = adress;
            Port = port;
            genGUID();
            Tasks = new List<Task>();
            State = NodeState.WAIT;
            Results = new List<Tuple<int, List<Object>>>();
        }

        public Node(string name, string adress, int port, Socket sock)
        {
            Name = name;
            Address = adress;
            Port = port;
            NodeSocket = sock;
            Tasks = new List<Task>();
            State = NodeState.WAIT;
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
                if (obj.Method != GET_CPU_METHOD)
                {
                    Console.WriteLine("Send data : " + obj + " to : " + node);
                }
                node.NodeSocket.BeginSend(data, 0, data.Length, 0,
                    new AsyncCallback(SendCallback),node);
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
                Node node = (Node)ar.AsyncState;
                // Retrieve the socket from the state object.
                Socket client = node.NodeSocket;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
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
                StateObject obj = new StateObject();
                obj.node = node;
                node.NodeSocket.BeginReceive(obj.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), obj);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public List<String> GetMonitoringInfos(Node n)
        {
            try
            {
                string[] info = n.NodeGUID.Split(':');
                List<string> list = new List<string>
                {
                    info[0],
                    info[1],
                    info[2]
                };
                return list;
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        public void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject stateObj = (StateObject)ar.AsyncState;
                Node node = stateObj.node;

                // Read data from the remote device.
                int bytesRead = node.NodeSocket.EndReceive(ar);

                stateObj.data.Add(stateObj.buffer);
                try
                {
                    byte[] data = stateObj.data
                                     .SelectMany(a => a)
                                     .ToArray();
                    DataInput input = DataFormater.Deserialize<DataInput>(data);
                    Receive(node);
                    ProcessInput(input, node);
                }
                catch(Exception e)
                {
                    node.NodeSocket.BeginReceive(stateObj.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), stateObj);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public abstract void ProcessInput(DataInput input,Node node);    

        public override string ToString()
        {
            return "Node -> Address : " + Address + " Port : " + Port + " NodeGuid : " + NodeGUID; 
        }

        protected void genGUID()
        {
            NodeGUID =  Name +":" + Address + ":" + Port;
        }

        protected void UpdateResult(Object input, int taskId)
        {
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].Item1 == taskId)
                {
                    List<Object> list = results[i].Item2;
                    list.Add(input);
                    results[i] = new Tuple<int, List<object>>(results[i].Item1, list);                  
                }
            }
        }
        protected List<Object> GetResultFromTaskId(int taskId)
        {
            foreach (Tuple<int, List<Object>> result in Results)
            {
                if (result.Item1 == taskId)
                {
                    return result.Item2;
                }
            }
            throw new Exception("Aucune ligne de résultat ne correspond à cette tâche");
        }

    }
}
