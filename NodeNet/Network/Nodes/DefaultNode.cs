using NodeNet.Data;
using NodeNet.Map_Reduce;
using NodeNet.Network.States;
using NodeNet.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using NodeNet.Utilities;

namespace NodeNet.Network.Nodes
{
    /// <summary>
    /// Classe définissant le comportement par des défaut des noeurds de l'architecture
    /// </summary>
    public class DefaultNode : Node
    {
        #region Properties

        /// <summary>
        /// Liste de l'état des tâches en cours de traitement
        /// </summary>
        public List<Tuple<int, int, NodeState>> ProcessingTask { get; set; }

        // int -> id de la worker task, int -> id de la task, NodeStatus -> Status de la WorkerTask
        /// <summary>
        /// Liste de l'état des tâches et de l'état des workers éxécutant ces tâches
        /// </summary>
        public ConcurrentDictionary<int, Tuple<int, NodeState>> WorkerTaskStatus { get; set; }

        /// <summary>
        /// Active le monitoring des tâches
        /// </summary>
        bool monitoringEnable = true;
        #endregion

        /// <summary>
        /// Constructeur initialisant l'adresse IP, le port et le nom du noeud ainsi que les worker infrastructure IDENT et GET_CPU
        /// </summary>
        /// <param name="name">Nom du noeud</param>
        /// <param name="adress">Adresse IP du noeud</param>
        /// <param name="port">Port d'écoute</param>
        public DefaultNode(string name, string adress, int port, bool enabled) : base(name, adress, port)
        {

            Logger = new Logger(enabled);

            WorkerTaskStatus = new ConcurrentDictionary<int, Tuple<int, NodeState>>();
            WorkerFactory = TaskExecFactory.GetInstance();
            try
            {
                WorkerFactory.AddWorker(IdentMethod, new TaskExecutor(this, ProcessIndent, null, null));
                WorkerFactory.AddWorker(GetCpuMethod, new TaskExecutor(this, StartMonitoring, null, null));
            }
            catch (Exception e)
            {
                Logger.Write(e);
            }
        }

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        /// <param name="name">Nom du noeud</param>
        /// <param name="adress">Adresse IP du noeud</param>
        /// <param name="port">Port d'écoute</param>
        /// <param name="sock">Socket d'écoute</param>
        public DefaultNode(string name, string adress, int port, Socket sock) : base(name, adress, port, sock) { }

        /// <summary>
        /// Méthode de réception des objets de transfert de données permettant d'initialiser un TaskExecutor avec le nom de la méthode reçue dans l'objet de transfert
        /// </summary>
        /// <param name="input">Objet de transfert contenant la méthode</param>
        /// <param name="node">Noeud emetteur</param>
        public override void ProcessInput(DataInput input, Node node)
        {
            Logger.Write($"Traitement en cours pour : {input.Method}.");
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
            if (!input.Method.Equals(IdentMethod) && !input.Method.Equals(GetCpuMethod))
            {
                // Creation d'une nouvelle task
                Tasks.TryAdd(input.NodeTaskId, new Task(input.NodeTaskId, NodeState.Wait));
                Results.TryAdd(input.NodeTaskId, new ConcurrentBag<object>());
            }
            object res = executor.DoWork(input);
            if (res != null)
            {
                DataInput resp = new DataInput()
                {
                    ClientGuid = input.ClientGuid,
                    NodeGuid = NodeGuid,
                    TaskId = input.TaskId,
                    NodeTaskId = input.NodeTaskId,
                    Method = input.Method,
                    Data = res,
                    MsgType = MessageType.Response
                };
                SendData(node, resp);
            }
        }
   
        #region Worker method

        /// <summary>
        /// Méthode d'identification à l'orchestrateur et de réponse à la réquête d'identification
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private DataInput ProcessIndent(DataInput d)
        {
            Tuple<string, int> orchIDentifiers = (Tuple<string, int>)d.Data;
            Name = Name + orchIDentifiers.Item1;
            Port = orchIDentifiers.Item2;
            GenGuid();
            DataInput resp = new DataInput()
            {
                ClientGuid = null,
                TaskId = d.TaskId,
                NodeGuid = NodeGuid,
                MsgType = MessageType.Response,
                Method = d.Method
            };
            return resp;
        }
        /// <summary>
        /// Méthode de démarrage du monitoring des performances machine
        /// </summary>
        /// <param name="input">objet de trasnfert à réinitialiser avbec les informations du noeud</param>
        /// <returns>informations de monitorng contenues dans l'objetde transfert</returns>
        private object StartMonitoring(DataInput input)
        {

            ManagementObjectSearcher wmiObject = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            if (PerfCpu == null)
            {
                PerfCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            }
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            bw.DoWork += (o, a) =>
            {
                while (monitoringEnable)
                {
                    float cpuCount = PerfCpu.NextValue();
                    var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
                    {
                        FreePhysicalMemory = double.Parse(mo["FreePhysicalMemory"].ToString()),
                        TotalVisibleMemorySize = double.Parse(mo["TotalVisibleMemorySize"].ToString())
                    }).FirstOrDefault();
                    double ramCount = 0;
                    if (memoryValues != null)
                    {
                        ramCount = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
                    }
                    DataInput perfInfo = new DataInput()
                    {
                        ClientGuid = input.ClientGuid,
                        NodeGuid = NodeGuid,
                        MsgType = MessageType.Response,
                        Method = GetCpuMethod,
                        TaskId = input.TaskId,
                        Data = new Tuple<float, double>(cpuCount, ramCount)
                    };
                    SendData(Orch, perfInfo);
                    Thread.Sleep(3000);
                }
            };
            bw.RunWorkerAsync();
            return null;
        }

        /// <summary>
        /// Méthode exécutant le traitement demandé dans l'objet de transfert au travers de gestionnaire de thread indépendant
        /// </summary>
        /// <param name="processFunction">Méthode à exécutant la fonction apssé&e en paramètre de l'objet de transfert </param>
        /// <param name="taskData">Objet dfe trasnfert contenant les données à traiter </param>
        /// <param name="totalNbWorker"> Nombre de gestionbnaires de thread à exécuter</param>
        protected void LaunchBgForWork(Action<object, DoWorkEventArgs> processFunction, DataInput taskData, int totalNbWorker)
        {
            BackgroundWorker bw = new BackgroundWorker();

            //// Abonnement ////
            bw.DoWork += new DoWorkEventHandler(processFunction);
            bw.RunWorkerCompleted += WorkerEndProcess;
            int workerTaskId = CreateWorkerTask(taskData.NodeTaskId);
            Tuple<int, DataInput, int> dataAndMeta = new Tuple<int, DataInput, int>(workerTaskId, taskData, totalNbWorker);
            bw.RunWorkerAsync(dataAndMeta);
        }

        /// <summary>
        /// Méthode retournant le pourcentage de progression d'une tâche en cours 
        /// </summary>
        /// <param name="nodeTaskId">ID de la tâche en cours de ce noeud</param>
        /// <param name="totalNbWorker">Nombre total de BackgroundWorkers de ce noeud</param>
        /// <returns></returns>
        private double GetWorkersProgression(int nodeTaskId, int totalNbWorker)
        {
            int nbWorkerEnd = WorkerTaskStatus.Count(item => item.Value.Item1 == nodeTaskId && item.Value.Item2 == NodeState.Finish);
            return nbWorkerEnd * 100 / totalNbWorker;
        }

        /// <summary>
        /// Méthode d'écoute de fin d'exécution des BackgroundWorkers
        /// </summary>
        /// <param name="sender">BackgroundWorker emetteur</param>
        /// <param name="e">résultat de fin de traitement</param>
        protected void WorkerEndProcess(object sender, RunWorkerCompletedEventArgs e)
        {
            // Manage if e.Error != null
            Tuple<int, DataInput, int> data = (Tuple<int, DataInput, int>)e.Result;
            DataInput resp = data.Item2;
            UpdateWorkerTaskStatus(data.Item1, data.Item2.NodeTaskId, NodeState.Finish);
            UpdateResult(resp.Data, data.Item2.NodeTaskId, data.Item1);
            if (TaskIsCompleted(resp.NodeTaskId))
            {
                Logger.Write($"Traitement terminé pour {data.Item2.Method}.");
                Logger.Write(@"Résultat envoyé à l'orchestrateur.");
                IReducer reducer = WorkerFactory.GetWorker(resp.Method).Reducer;
                object result = reducer.Reduce(GetResultFromTaskId(resp.NodeTaskId));
                resp.Data = result;
                SendData(Orch, resp);
            }
            else
            {
                double progression = GetWorkersProgression(data.Item2.NodeTaskId, data.Item3);
                DataInput status = new DataInput()
                {
                    ClientGuid = data.Item2.ClientGuid,
                    NodeGuid = NodeGuid,
                    MsgType = MessageType.Call,
                    Method = TaskStatusMethod,
                    TaskId = data.Item2.TaskId,
                    NodeTaskId = data.Item2.NodeTaskId,
                    Data = new Tuple<NodeState, object>(NodeState.Work, progression)
                };
                SendData(Orch, status);
            }
        }

        #endregion

        #region Utilitary Methods
        /// <summary>
        /// Mise à jour de la task dans la liste Tasks (ajout d'un nouveau WorkerTaskID) si elle existe ou création d'une nouvelle.
        /// </summary>
        /// <param name="taskId">L'ID de la tâche</param>
        /// <returns>l'id de la workerTask crée</returns>
        private int CreateWorkerTask(int taskId)
        {
            int lastWorkerTaskId = LastSubTaskId;
            if (Tasks.TryGetValue(taskId, out Task _))
            {
                WorkerTaskStatus.TryAdd(lastWorkerTaskId, new Tuple<int, NodeState>(taskId, NodeState.Work));
                return lastWorkerTaskId;
            }
            else
            {
                throw new Exception("Aucune Task trouvé dans la liste Tasks du Node pour cet ID : " + taskId);
            }

        }

        /// <summary>
        /// Méthode de mise à jour du statut d'une tâche au sein de ce noeud
        /// </summary>
        /// <param name="workerTaskId">ID de la tâche exécutée par le TaskExecutor</param>
        /// <param name="nodeTaskId">ID de la tâche reçue par le noeud</param>
        /// <param name="status">Statut du noeud</param>
        private void UpdateWorkerTaskStatus(int workerTaskId, int nodeTaskId, NodeState status)
        {
            Tuple<int, NodeState> updatedWorkerTask = new Tuple<int, NodeState>(nodeTaskId, status);
            if (WorkerTaskStatus.TryGetValue(workerTaskId, out Tuple<int, NodeState> workerTask))
            {
                WorkerTaskStatus.TryUpdate(workerTaskId, updatedWorkerTask, workerTask);
            }
        }

        /// <summary>
        /// Méthode d'encapsulation de donnée à trasnfgérer dans un objet de transfert
        /// </summary>
        /// <param name="input">Objet de transfert destiné à contenir la donnée</param>
        /// <param name="data">Donnée</param>
        /// <returns>Objet de transfert contenant la donnée</returns>
        public DataInput PrepareData(DataInput input, object data)
        {
            DataInput duplicate = new DataInput()
            {
                ClientGuid = input.ClientGuid,
                NodeGuid = NodeGuid,
                TaskId = input.TaskId,
                NodeTaskId = input.NodeTaskId,
                Method = input.Method,
                MsgType = MessageType.Response,
                Data = data
            };
            return duplicate;
        }

        private bool TaskIsCompleted(int taskId)
        {
            bool completed = true;
            foreach (var item in WorkerTaskStatus)
            {
                if (item.Value.Item1 == taskId && item.Value.Item2 != NodeState.Finish)
                {
                    completed = false;
                }
            }
            return completed;
        }

        /// <summary>
        /// Méthode d'affichage du message de fermeture de l'application 
        /// </summary>
        /// <param name="node">Noeud en arrêt</param>
        public override void RemoveDeadNode(Node node)
        {
            MessageBox.Show(@"Erreur sur l'orchestrateur. Fermeture de l'application...");
            Process.GetCurrentProcess().CloseMainWindow();
        }

        #endregion
    }
}
