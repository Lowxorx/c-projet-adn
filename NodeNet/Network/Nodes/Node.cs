using NodeNet.Data;
using NodeNet.Network.Orch;
using NodeNet.Network.States;
using NodeNet.Tasks;
using NodeNet.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        #region Properties
        public String NodeGUID;

        public Node Orch { get; set; }
        private Logger logger;
        public String Address { get; set; }
        public int Port { get; set; }
        public String Name { get; set; }
        public Socket NodeSocket { get; set; }
        public Socket ServerSocket { get; set; }
        public static int BUFFER_SIZE = 4096;
        public PerformanceCounter PerfCpu { get; set; }
        public PerformanceCounter PerfRam { get; set; }
        public NodeState State { get; set; }
        private ConcurrentDictionary<int, Task> tasks;
        public ConcurrentDictionary<int, Task> Tasks
        {
            get { return tasks; }
            set { tasks = value; }
        }
        /* Stockage des résultats réduits par Task */
        private ConcurrentDictionary<int, ConcurrentBag<Object>> results;
        public ConcurrentDictionary<int, ConcurrentBag<Object>> Results
        {
            get { return results; }
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
        #endregion

        #region Ctor
        public Node(String name, String adress, int port)
        {
            WorkerFactory = TaskExecFactory.GetInstance();
            Name = name;
            Address = adress;
            Port = port;
            GenGUID();
            Tasks = new ConcurrentDictionary<int, Task>();
            State = NodeState.WAIT;
            Results = new ConcurrentDictionary<int, ConcurrentBag<object>>();
            logger = new Logger();
        }

        public Node(string name, string adress, int port, Socket sock)
        {
            Name = name;
            Address = adress;
            Port = port;
            NodeSocket = sock;
            Tasks = new ConcurrentDictionary<int, Task>();
            State = NodeState.WAIT;
            logger = new Logger();
        }
        #endregion

        #region Methods
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
                logger.Write(e, true);
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
                logger.Write(String.Format("Connection accepted : {0} ", client.RemoteEndPoint.ToString()), true);
                Console.WriteLine(String.Format("Connection accepted : {0} ", client.RemoteEndPoint.ToString()));
                // Signal that the connection has been made.
                connectDone.Set();
                Receive(Orch);
            }
            catch (SocketException e)
            {
                logger.Write(e, true);
                Console.WriteLine(e.ToString());
            }
        }

        public void SendData(Node node, DataInput obj)
        {
            byte[] data = DataFormater.Serialize(obj);
            // TODO TCP.Connected
            try
            {
                if (obj.Method != GET_CPU_METHOD)
                {
                    logger.Write(String.Format("Send data : {0} to {1}", obj, node), true);
                    Console.WriteLine("Send data : " + obj + " to : " + node);
                }
                node.NodeSocket.BeginSend(data, 0, data.Length, 0,new AsyncCallback(SendCallback), node);
            }
            catch (SocketException e)
            {
                /// Client Down ///
                if (!node.NodeSocket.Connected)
                {
                    logger.Write(e, true);
                    logger.Write(String.Format("Client {0} disconnected", node.NodeSocket.RemoteEndPoint.ToString()), true);
                    Console.WriteLine("Client " + node.NodeSocket.RemoteEndPoint.ToString() + " Disconnected");
                    RemoveDeadNode(node);
                }
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
                logger.Write(e, true);
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
                node.NodeSocket.BeginReceive(obj.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), obj);
            }
            catch (Exception e)
            {
                logger.Write(e, true);
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
                try
                {
                    // Read data from the remote device.
                    int nbByteRead = node.NodeSocket.EndReceive(ar);
                    // Gety data from buffer
                    byte[] dataToConcat = new byte[nbByteRead];
                    Array.Copy(stateObj.buffer, 0, dataToConcat, 0, nbByteRead);
                    stateObj.data.Add(dataToConcat);
                    if (IsEndOfMessage(stateObj.buffer, nbByteRead))
                    {
                        byte[] data = ConcatByteArray(stateObj.data);
                        DataInput input = DataFormater.Deserialize<DataInput>(data);
                        Receive(node);
                        ProcessInput(input, node);
                    }
                    else
                    {
                        node.NodeSocket.BeginReceive(stateObj.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), stateObj);
                    }
                }
                catch (SocketException e)
                {
                    logger.Write(e, true);
                    RemoveDeadNode(node);
                }
            }
            catch (Exception e)
            {
                logger.Write(e, true);
                Console.WriteLine(e.ToString());
            }
        }

        private byte[] ConcatByteArray(List<byte[]> data)
        {
            List<byte> byteStorage = new List<byte>();
            foreach (byte[] bytes in data)
            {
                foreach (byte bit in bytes)
                {
                    byteStorage.Add(bit);
                }
            }
            return byteStorage.ToArray();
        }

        private bool IsEndOfMessage(byte[] buffer, int byteRead)
        {
            byte[] endSequence = Encoding.ASCII.GetBytes("CAFEBABE");
            byte[] endOfBuffer = new byte[8];
            Array.Copy(buffer, byteRead - endSequence.Length, endOfBuffer, 0, endSequence.Length);
            return endSequence.SequenceEqual(endOfBuffer);
        }

        public abstract void ProcessInput(DataInput input, Node node);

        public override string ToString()
        {
            return "Node -> Address : " + Address + " Port : " + Port + " NodeGuid : " + NodeGUID;
        }

        protected void GenGUID()
        {
            NodeGUID = Name + ":" + Address + ":" + Port;
        }

        protected void UpdateResult(Object input, int taskId, int subTaskId)
        {
            Console.WriteLine("Update result");
            if (Results.TryGetValue(taskId, out ConcurrentBag<object> result))
            {
                result.Add(new Tuple<int, Object>(subTaskId, input));
            }
            else
            {
                throw new Exception("Aucune Task avec cet id ");
            }
        }
        protected ConcurrentBag<Object> GetResultFromTaskId(int taskId)
        {
            ConcurrentBag<Object> result;
            if (Results.TryGetValue(taskId, out result))
            {
                return result;
            }
            throw new Exception("Aucune ligne de résultat ne correspond à cette tâche");
        }

        public abstract void RemoveDeadNode(Node node);

        protected bool MethodIsNotInfra(String method)
        {
            return method != GET_CPU_METHOD && method != IDENT_METHOD && method != TASK_STATUS_METHOD;
        }
        #endregion 



    }
}
