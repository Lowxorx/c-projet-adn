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
    /// <summary>
    /// Objet rerésentant l'état d'une connexion par Socket
    /// </summary>
    public class StateObject
    {
        /// <summary>
        /// Noeud définit
        /// </summary>
        public Node Node;
        /// <summary>
        /// Taille maximale du buffer
        /// </summary>
        public const int BufferSize = 4096;
        /// <summary>
        /// Buffer de réception
        /// </summary>
        public byte[] Buffer = new byte[BufferSize];
        /// <summary>
        /// Données reçues
        /// </summary>
        public List<byte[]> Data = new List<byte[]>();
    }
    /// <summary>
    /// Objet représentant le noeud au sein d'une architecture clusterisée
    /// </summary>
    public abstract class Node : INode
    {
        #region Properties
        /// <summary>
        /// ID du Noeud
        /// </summary>
        public string NodeGuid;
        /// <summary>
        /// Noeud Orchestrateur auquel ce noeud est connecté
        /// </summary>
        public Node Orch { get; set; }
        /// <summary>
        /// Objet de log
        /// </summary>
        public Logger Logger { get; set; }
        /// <summary>
        /// Adresse ip de ce noeud
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Port d'écoute
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// Nom de ce noeud
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Socket d'écoute de ce noeud
        /// </summary>
        public Socket NodeSocket { get; set; }
        /// <summary>
        /// Socket de destination
        /// </summary>
        public Socket ServerSocket { get; set; }
        /// <summary>
        /// Taille maximale du buffer
        /// </summary>
        public static int BufferSize = 4096;
        /// <summary>
        /// Compteur des performances des coeurs processeur
        /// </summary>
        public PerformanceCounter PerfCpu { get; set; }
        /// <summary>
        /// Compteur des performances de la mémoire vive
        /// </summary>
        public PerformanceCounter PerfRam { get; set; }
        /// <summary>
        /// Etat du noeud
        /// </summary>
        public NodeState State { get; set; }
        /// <summary>
        /// Liste des tâches reçues par ce noeud
        /// </summary>
        public ConcurrentDictionary<int, Task> Tasks { get; set; }
        /// <summary>
        /// Liste des résultats réduits pour chaque tâche
        /// </summary>
        public ConcurrentDictionary<int, ConcurrentBag<object>> Results { get; set; }
        /// <summary>
        /// ID de la dernière tâche attribuée à ce noeud
        /// </summary>
        private int lastTaskId;
        protected int LastTaskId => lastTaskId += 1;

        /// <summary>
        /// ID de la dernière sous tâche de la tâche attribuée
        /// </summary>
        private int lastSubTaskId;
        protected int LastSubTaskId => lastSubTaskId += 1;

        /// <summary>
        /// Valeur d'utilisation du processeur
        /// </summary>
        private float CpuVal { get; set; }
        public float CpuValue { get => (float)(Math.Truncate(CpuVal * 100.0) / 100.0);
            set => CpuVal = value;
        }
        /// <summary>
        /// Valeur d'utilisation de la mémoire vive
        /// </summary>
        private double RamVal { get; set; }
        public double RamValue { get => (Math.Truncate(RamVal * 100.0) / 100.0);
            set => RamVal = value;
        }
        /// <summary>
        /// Tâche actuelle en traitement
        /// </summary>
        public int WorkingTask { get; set; }

        /// <summary>
        /// % de progression du traitement actuel
        /// </summary>
        public double Progression { get; set; }

        /// <summary>
        /// WorkerFactory permettant de produire les TaskExecutor
        /// </summary>
        public TaskExecFactory WorkerFactory { get; set; }
        /// <summary>
        /// Trame de transfert
        /// </summary>
        protected List<byte[]> BytearrayList { get; set; }
        /// <summary>
        /// Evennement de gestion des connexions Socket asynchrones permettant de définir l'arrêt d'un envoi asynchrone
        /// </summary>
        protected static ManualResetEvent SendDone = new ManualResetEvent(false);
        /// <summary>
        /// Evennement de gestion des connexions Socket asynchrones permettant de définir l'arrêt d'une réception asynchrone
        /// </summary>
        protected static ManualResetEvent ReceiveDone = new ManualResetEvent(false);
        /// <summary>
        /// Evennement de gestion des connexions Socket asynchrones permettant de définir l'arrêt d'une connexion asynchrone
        /// </summary>
        protected static ManualResetEvent ConnectDone = new ManualResetEvent(false);
        /// <summary>
        /// Constante définissant le nom de la méthode dynamique GET_CPU
        /// </summary>
        protected const string GetCpuMethod = "GET_CPU";
        /// <summary>
        /// Constante définissant le nom de la méthode dynamique IDENT
        /// </summary>
        protected const string IdentMethod = "IDENT";
        /// <summary>
        /// Constante définissant le nom de la méthode dynamique TASK_STATE
        /// </summary>
        protected const string TaskStatusMethod = "TASK_STATE";
        #endregion

        #region Ctor
        /// <summary>
        /// Constructeur initialisant les tâches et la liste de résultats
        /// </summary>
        /// <param name="name">Nom du noeud</param>
        /// <param name="adress">adresse IP du noeud</param>
        /// <param name="port">port d'écoute du noeud</param>
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

        /// <summary>
        /// Constructeur permettant de définir le socket d'écoute du noeud
        /// </summary>
        /// <param name="name">Nom du noeud</param>
        /// <param name="adress">adresse IP du noeud</param>
        /// <param name="port">port d'écoute du noeud</param>
        /// <param name="sock">Socket</param>

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
        /// <summary>
        /// Méthode d'arrêt du noeud
        /// </summary>
        public void Stop()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Méthode définissant l'orchestrateur auquel ce noeud est connecté
        /// </summary>
        /// <param name="node"></param>
        public void RegisterOrch(Orchestrator node)
        {
            Orch = node;
        }

        /// <summary>
        /// Méthode de connexion asynchrone à un serveur distant
        /// </summary>
        /// <param name="address">adresse IP serveur distant</param>
        /// <param name="port">port d'écoute du serveur distant</param>
        public void Connect(string address, int port)
        {
            IPEndPoint remoteEp = new IPEndPoint(IPAddress.Parse(address), port);
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Orch = new DefaultNode("Orch", address, port, ServerSocket);
            try
            {
                ServerSocket.BeginConnect(remoteEp, ConnectCallback, Orch);
                ConnectDone.WaitOne();
            }
            catch (Exception e)
            {
                Logger.Write(e);
            }
        }

        /// <summary>
        /// Méthode d'écoute du retour de la connexion asynchrone
        /// </summary>
        /// <param name="ar">résultat de l'évennement de retour</param>
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

        /// <summary>
        /// Méthode d'envoi de données à un hôte distant en mode asynchrone
        /// </summary>
        /// <param name="node">Noeud distant</param>
        /// <param name="obj">Objet de transfert de la donnée</param>
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
                    RemoveDeadNode(node);
                }
            }
        }

        /// <summary>
        /// Méthode d'écoute du retour de l'envoi asynchrone
        /// </summary>
        /// <param name="ar">Résultat de l'envoi asynchrone</param>
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
            }
        }

        /// <summary>
        /// Méthode de réception de données asynchrone depuis un hôte distant
        /// </summary>
        /// <param name="node">hôte distant émetteur</param>
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
            }
        }

        /// <summary>
        /// Méthode permettant de connaître les détails d'un noeud à partir de son NodeGuid
        /// </summary>
        /// <param name="n">Noeud dont on nécessite les informations</param>
        /// <returns>Liste des informations du noeud</returns>
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

        /// <summary>
        /// Méthode d'écoute de la réception de données asynchrone depuis un hôte distant
        /// </summary>
        /// <param name="ar">Résultat de la réception asynchrone</param>
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
            }
        }

        /// <summary>
        /// Méthode de concaténation d'une liste de tableaux d'octets
        /// </summary>
        /// <param name="data">liste de tableaux d'octets à concaténer</param>
        /// <returns>tableaux d'octets contenant tous les tableaux concaténés de la liste</returns>
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

        /// <summary>
        /// Méthode permettant de déterminer si toutes les trames composant un message ont été reçues
        /// </summary>
        /// <param name="buffer">buffer actuel</param>
        /// <param name="byteRead">nombre d'octets lus</param>
        /// <returns>indique fin du message ou non</returns>
        private bool IsEndOfMessage(byte[] buffer, int byteRead)
        {
            byte[] endSequence = Encoding.ASCII.GetBytes("CAFEBABE");
            byte[] endOfBuffer = new byte[8];
            Array.Copy(buffer, byteRead - endSequence.Length, endOfBuffer, 0, endSequence.Length);
            return endSequence.SequenceEqual(endOfBuffer);
        }

        /// <summary>
        /// Méthode héritée de gestion de la réception d'un objet de transfert de données
        /// </summary>
        /// <param name="input">objet de transfert de données</param>
        /// <param name="node">hôte distant émetteur</param>
        public abstract void ProcessInput(DataInput input, Node node);

        public override string ToString()
        {
            return "Node -> Address : " + Address + " Port : " + Port + " NodeGuid : " + NodeGuid;
        }
        /// <summary>
        /// Méthode de concaténation de la valeur du NodeGuid
        /// </summary>
        protected void GenGuid()
        {
            NodeGuid = Name + ":" + Address + ":" + Port;
        }

        /// <summary>
        /// Méthode mettant à jour les résultats d'une tâche donnée
        /// </summary>
        /// <param name="input">objet de transfert de données</param>
        /// <param name="taskId">ID de la tâche</param>
        /// <param name="subTaskId">ID de la sous tâche</param>
        protected void UpdateResult(object input, int taskId, int subTaskId)
        {
            if (Results.TryGetValue(taskId, out ConcurrentBag<object> result))
            {
                result.Add(new Tuple<int, object>(subTaskId, input));
            }
            else
            {
                throw new Exception("Aucune Task avec cet id ");
            }
        }
        /// <summary>
        /// Méthode récupérant les résultats d'une tâche donnée
        /// </summary>
        /// <param name="taskId">ID de la tâche</param>
        /// <returns>Liste des résultats de la tâche</returns>
        protected ConcurrentBag<object> GetResultFromTaskId(int taskId)
        {
            if (Results.TryGetValue(taskId, out ConcurrentBag<object> result))
            {
                return result;
            }
            throw new Exception("Aucune ligne de résultat ne correspond à cette tâche");
        }

        /// <summary>
        /// Méthode héritée supprimant un noeud déconnecté à ce noeud
        /// </summary>
        /// <param name="node">Noeud déconnecté</param>
        public abstract void RemoveDeadNode(Node node);

        /// <summary>
        /// Méthode déterminant si une méthode dynamique demandée fait partie des méthodes dynamiques d'infrastructure
        /// </summary>
        /// <param name="method">nom de la méthode dynamique</param>
        /// <returns>fait partie de l'infrastructure ou non</returns>
        protected bool MethodIsNotInfra(string method)
        {
            return method != GetCpuMethod && method != IdentMethod && method != TaskStatusMethod;
        }
        #endregion 



    }
}
