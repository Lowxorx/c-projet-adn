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
        public Node Node;
        // Size of receive buffer.
        public const int BufferSize = 4096;
        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];
        // Received data
        public List<byte[]> Data = new List<byte[]>();
    }
    public abstract class Node : INode
    {
        #region Properties
        public string NodeGuid;

        public Node Orch { get; set; }
        public Logger Logger { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Name { get; set; }
        public Socket NodeSocket { get; set; }
        public Socket ServerSocket { get; set; }
        public static int BufferSize = 4096;
        public PerformanceCounter PerfCpu { get; set; }
        public PerformanceCounter PerfRam { get; set; }
        public NodeState State { get; set; }
        public ConcurrentDictionary<int, Task> Tasks { get; set; }

        /* Stockage des résultats réduits par Task */
        public ConcurrentDictionary<int, ConcurrentBag<object>> Results { get; set; }

        private int lastTaskId;
        protected int LastTaskId
        {
            get { return lastTaskId += 1; }
            set { new InvalidOperationException(); }
        }
        private int lastSubTaskId;
        protected int LastSubTaskId
        {
            get { return lastSubTaskId += 1; }
            set { new InvalidOperationException(); }
        }
        private float CpuVal { get; set; }
        public float CpuValue { get => (float)(Math.Truncate(CpuVal * 100.0) / 100.0);
            set => CpuVal = value;
        }
        private double RamVal { get; set; }
        public double RamValue { get => (Math.Truncate(RamVal * 100.0) / 100.0);
            set => RamVal = value;
        }

        public int WorkingTask { get; set; }

        public double Progression { get; set; }

        public TaskExecFactory WorkerFactory { get; set; }
        protected List<byte[]> BytearrayList { get; set; }
        protected static ManualResetEvent SendDone = new ManualResetEvent(false);
        protected static ManualResetEvent ReceiveDone = new ManualResetEvent(false);
        protected static ManualResetEvent ConnectDone = new ManualResetEvent(false);
        protected const string GetCpuMethod = "GET_CPU";
        protected const string IdentMethod = "IDENT";
        protected const string TaskStatusMethod = "TASK_STATE";
        #endregion

        #region Ctor
        protected Node(string name, string adress, int port)
        {
            WorkerFactory = TaskExecFactory.GetInstance();
            Name = name;
            Address = adress;
            Port = port;
            GenGuid();
            Tasks = new ConcurrentDictionary<int, Task>();
            State = NodeState.Wait;
            Results = new ConcurrentDictionary<int, ConcurrentBag<object>>();
        }

        protected Node(string name, string adress, int port, Socket sock)
        {
            Name = name;
            Address = adress;
            Port = port;
            NodeSocket = sock;
            Tasks = new ConcurrentDictionary<int, Task>();
            State = NodeState.Wait;
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
            IPEndPoint remoteEp = new IPEndPoint(IPAddress.Parse(address), port);
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Orch = new DefaultNode("Orch", address, port, ServerSocket);
            Console.WriteLine(@"Launch Cli");
            try
            {
                ServerSocket.BeginConnect(remoteEp, ConnectCallback, Orch);
                ConnectDone.WaitOne();
            }
            catch (Exception e)
            {
                Logger.Write(e);
                Console.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            Node orch = (Node)ar.AsyncState;
            try
            {
                // Complete the connection.
                orch.NodeSocket.EndConnect(ar);
                Logger.Write($"Connecté à l'orchestrateur : { orch.NodeSocket.RemoteEndPoint} ");
                // Signal that the connection has been made.
                ConnectDone.Set();
                Receive(Orch);
            }
            catch (SocketException)
            {
                ConnectDone.Set();
                Logger.Write("Hote distant injoignable, nouvel essai dans 3 secondes ...  ");
                Thread.Sleep(3000);
                Connect(orch.Address, orch.Port);
            }
        }

        public void SendData(Node node, DataInput obj)
        {
            byte[] data = DataFormater.Serialize(obj);
            try
            {
                node.NodeSocket.BeginSend(data, 0, data.Length, 0,SendCallback, node);
            }
            catch (SocketException e)
            {
                // Client Down 
                if (!node.NodeSocket.Connected)
                {
                    Logger.Write(e);
                    Logger.Write($"Client {node.NodeSocket.RemoteEndPoint} disconnected");
                    Console.WriteLine(@"Client " + node.NodeSocket.RemoteEndPoint + @" Disconnected");
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
                client.EndSend(ar);
                // Signal that all bytes have been sent.
                SendDone.Set();
            }
            catch (Exception e)
            {
                Logger.Write(e);
                Console.WriteLine(e.ToString());
            }
        }

        public void Receive(Node node)
        {
            try
            {
                // Begin receiving the data from the remote device.
                StateObject obj = new StateObject {Node = node};
                node.NodeSocket.BeginReceive(obj.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, obj);
            }
            catch (Exception e)
            {
                Logger.Write(e);
                Console.WriteLine(e.ToString());
            }
        }

        public List<string> GetMonitoringInfos(Node n)
        {
            try
            {
                string[] info = n.NodeGuid.Split(':');
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
                Node node = stateObj.Node;
                try
                {
                    // Read data from the remote device.
                    int nbByteRead = node.NodeSocket.EndReceive(ar);
                    // Gety data from buffer
                    byte[] dataToConcat = new byte[nbByteRead];
                    Array.Copy(stateObj.Buffer, 0, dataToConcat, 0, nbByteRead);
                    stateObj.Data.Add(dataToConcat);
                    if (IsEndOfMessage(stateObj.Buffer, nbByteRead))
                    {
                        byte[] data = ConcatByteArray(stateObj.Data);
                        DataInput input = DataFormater.Deserialize<DataInput>(data);
                        Receive(node);
                        ProcessInput(input, node);
                    }
                    else
                    {
                        node.NodeSocket.BeginReceive(stateObj.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, stateObj);
                    }
                }
                catch (SocketException e)
                {
                    Logger.Write(e);
                    RemoveDeadNode(node);
                }
            }
            catch (Exception e)
            {
                Logger.Write(e);
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
            return "Node -> Address : " + Address + " Port : " + Port + " NodeGuid : " + NodeGuid;
        }

        protected void GenGuid()
        {
            NodeGuid = Name + ":" + Address + ":" + Port;
        }

        protected void UpdateResult(object input, int taskId, int subTaskId)
        {
            Console.WriteLine(@"Launch Cli");
            if (Results.TryGetValue(taskId, out ConcurrentBag<object> result))
            {
                result.Add(new Tuple<int, object>(subTaskId, input));
            }
            else
            {
                throw new Exception("Aucune Task avec cet id ");
            }
        }
        protected ConcurrentBag<object> GetResultFromTaskId(int taskId)
        {
            ConcurrentBag<object> result;
            if (Results.TryGetValue(taskId, out result))
            {
                return result;
            }
            throw new Exception("Aucune ligne de résultat ne correspond à cette tâche");
        }

        public abstract void RemoveDeadNode(Node node);

        protected bool MethodIsNotInfra(string method)
        {
            return method != GetCpuMethod && method != IdentMethod && method != TaskStatusMethod;
        }
        #endregion 



    }
}
