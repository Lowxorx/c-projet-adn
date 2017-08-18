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

namespace NodeNet.Network.Nodes
{
    public class DefaultNode : Node
    {
        #region Properties
        private List<Tuple<int, int, NodeState>> processingTask;
        public List<Tuple<int, int, NodeState>> ProcessingTask
        {
            get { return processingTask; }
            set { processingTask = value; }
        }
        // int -> id de la worker task, int -> id de la task, NodeStatus -> Status de la WorkerTask
        private ConcurrentDictionary<int, Tuple<int, NodeState>> workerTaskStatus;
        public ConcurrentDictionary<int, Tuple<int, NodeState>> WorkerTaskStatus
        {
            get { return workerTaskStatus; }
            set { workerTaskStatus = value; }
        }
        bool monitoringEnable = true;

        #endregion

        public DefaultNode(String name, String adress, int port) : base(name, adress, port)
        {
            WorkerTaskStatus = new ConcurrentDictionary<int, Tuple<int, NodeState>>();
            WorkerFactory = TaskExecFactory.GetInstance();
            try
            {
                WorkerFactory.AddWorker(IDENT_METHOD, new TaskExecutor(this, ProcessIndent, null, null));
                WorkerFactory.AddWorker(GET_CPU_METHOD, new TaskExecutor(this, StartMonitoring, null, null));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public DefaultNode(string name, string adress, int port, Socket sock) : base(name, adress, port, sock) { }

        public override void ProcessInput(DataInput input, Node node)
        {
            Console.WriteLine("ProcessInput for " + input.Method);
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
            if (!input.Method.Equals(IDENT_METHOD) && !input.Method.Equals(GET_CPU_METHOD))
            {
                // Creation d'une nouvelle task
                Tasks.TryAdd(input.NodeTaskId, new Task(input.NodeTaskId, NodeState.WAIT));
                Results.TryAdd(input.NodeTaskId, new ConcurrentBag<object>());
            }
            Object res = executor.DoWork(input);
            if (res != null)
            {
                DataInput resp = new DataInput()
                {
                    ClientGUID = input.ClientGUID,
                    NodeGUID = NodeGUID,
                    TaskId = input.TaskId,
                    NodeTaskId = input.NodeTaskId,
                    Method = input.Method,
                    Data = res,
                    MsgType = MessageType.RESPONSE
                };
                SendData(node, resp);
            }
        }



        #region Worker method

        private DataInput ProcessIndent(DataInput d)
        {
            Tuple<String, int> orchIDentifiers = (Tuple<String, int>)d.Data;
            Name = Name + orchIDentifiers.Item1;
            Port = orchIDentifiers.Item2;
            genGUID();
            DataInput resp = new DataInput()
            {
                ClientGUID = null,
                TaskId = d.TaskId,
                NodeGUID = NodeGUID,
                MsgType = MessageType.RESPONSE,
                Method = d.Method
            };
            return resp;
        }

        private Object StartMonitoring(DataInput input)
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
                        FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                        TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
                    }).FirstOrDefault();
                    double ramCount = 0;
                    if (memoryValues != null)
                    {
                        ramCount = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
                    }
                    DataInput perfInfo = new DataInput()
                    {
                        ClientGUID = input.ClientGUID,
                        NodeGUID = NodeGUID,
                        MsgType = MessageType.RESPONSE,
                        Method = GET_CPU_METHOD,
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

        protected void LaunchBGForWork(Action<object, DoWorkEventArgs> ProcessFunction, DataInput taskData, int totalNbWorker)
        {
            BackgroundWorker bw = new BackgroundWorker();

            //// Abonnage ////
            bw.DoWork += new DoWorkEventHandler(ProcessFunction);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkerEndProcess);
            int workerTaskID = createWorkerTask(taskData.NodeTaskId);
            Tuple<int, DataInput, int> dataAndMeta = new Tuple<int, DataInput, int>(workerTaskID, taskData, totalNbWorker);
            bw.RunWorkerAsync(dataAndMeta);
        }

        private double getWorkersProgression(int nodeTaskID, int totalNbWorker)
        {
            int nbWorkerEnd = 0;
            foreach (var item in WorkerTaskStatus)
            {
                if (item.Value.Item1 == nodeTaskID && item.Value.Item2 == NodeState.FINISH)
                {
                    nbWorkerEnd++;
                }
            }
            return nbWorkerEnd * 100 / totalNbWorker;
        }

        protected void WorkerEndProcess(object sender, RunWorkerCompletedEventArgs e)
        {
            // Manage if e.Error != null
            Tuple<int, DataInput, int> data = (Tuple<int, DataInput, int>)e.Result;
            DataInput resp = data.Item2;
            updateWorkerTaskStatus(data.Item1, data.Item2.NodeTaskId, NodeState.FINISH);
            UpdateResult(resp.Data, data.Item2.NodeTaskId);
            if (TaskIsCompleted(resp.NodeTaskId))
            {
                Console.WriteLine("Task is completed");
                IReducer reducer = WorkerFactory.GetWorker(resp.Method).Reducer;
                Object result = reducer.reduce(GetResultFromTaskId(resp.NodeTaskId));
                resp.Data = result;
                SendData(Orch, resp);
            }
            else
            {
                //double progression = 0;
                //progression = getWorkersProgression(data.Item2.NodeTaskId, data.Item3);
                //Console.WriteLine("SendProgession to Orch : " + progression);
                //DataInput status = new DataInput()
                //{
                //    ClientGUID = data.Item2.ClientGUID,
                //    NodeGUID = NodeGUID,
                //    MsgType = MessageType.CALL,
                //    Method = TASK_STATUS_METHOD,
                //    TaskId = data.Item2.TaskId,
                //    NodeTaskId = data.Item2.NodeTaskId,
                //    Data = new Tuple<NodeState, Object>(NodeState.WORK, progression)
                //};
                //SendData(Orch, status);
            }
        }

        #endregion

        #region Utilitary Methods
        /* 
       * Mise à jour de la task dans la liste Tasks (ajout d'un nouveau WorkerTaskID)
       * si elle existe ou création d'une nouvelle.
       * @return retourne l'id de la workerTask crée
       */
        private int createWorkerTask(int taskID)
        {
            int lastWorkerTaskID = LastSubTaskID;
            Task task;
            if (Tasks.TryGetValue(taskID, out task))
            {
                WorkerTaskStatus.TryAdd(lastWorkerTaskID, new Tuple<int, NodeState>(taskID, NodeState.WORK));
                return lastWorkerTaskID;
            }
            else
            {
                throw new Exception("Aucune Task trouvé dans la liste Tasks du Node pour cet ID : " + taskID);
            }

        }

        private void updateWorkerTaskStatus(int workerTaskID, int nodeTaskId, NodeState status)
        {
            Console.WriteLine("Update Worker status : " + status);
            Tuple<int, NodeState> workerTask;
            Tuple<int, NodeState> updatedWorkerTask = new Tuple<int, NodeState>(nodeTaskId, status);
            if (WorkerTaskStatus.TryGetValue(workerTaskID, out workerTask))
            {
                WorkerTaskStatus.TryUpdate(workerTaskID, updatedWorkerTask, workerTask);
            }
        }

        public DataInput PrepareData(DataInput input, object data)
        {
            DataInput duplicate = new DataInput()
            {
                ClientGUID = input.ClientGUID,
                NodeGUID = NodeGUID,
                TaskId = input.TaskId,
                NodeTaskId = input.NodeTaskId,
                Method = input.Method,
                MsgType = MessageType.RESPONSE,
                Data = data
            };
            return duplicate;
        }

        private bool isFirstToStart(int taskID)
        {
            int nbWorking = 0;

            foreach (var item in WorkerTaskStatus)
            {
                if (item.Value.Item1 == taskID)
                {
                    nbWorking = item.Value.Item2 == NodeState.WORK ? nbWorking + 1 : nbWorking;
                }
            }

            return nbWorking == 1;
        }

        private bool TaskIsCompleted(int taskId)
        {
            bool completed = true;
            foreach (var item in WorkerTaskStatus)
            {
                if (item.Value.Item1 == taskId && item.Value.Item2 != NodeState.FINISH)
                {
                    completed = false;
                }
            }
            return completed;
        }
        #endregion
    }
}
