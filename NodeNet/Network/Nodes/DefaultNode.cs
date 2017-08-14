using NodeNet.Data;
using NodeNet.Map_Reduce;
using NodeNet.Network.States;
using NodeNet.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
        private List<Tuple<int,int, NodeState>> workerTaskStatus;
        public List<Tuple<int,int, NodeState>> WorkerTaskStatus
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return workerTaskStatus; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set { workerTaskStatus = value; }
        }
        bool monitoringEnable = true;

        #endregion

        public DefaultNode(String name, String adress, int port) : base(name, adress, port)
        {
            WorkerTaskStatus = new List<Tuple<int, int, NodeState>>();
            WorkerFactory = TaskExecFactory.GetInstance();
            try
            {
                WorkerFactory.AddWorker(IDENT_METHOD, new TaskExecutor(this, ProcessIndent,null,null));
                WorkerFactory.AddWorker(GET_CPU_METHOD, new TaskExecutor(this, StartMonitoring,null,null));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public DefaultNode(string name, string adress, int port, Socket sock) : base(name, adress, port, sock){}

        public override void ProcessInput(DataInput input, Node node)
        {
            Console.WriteLine("ProcessInput for " + input.Method);
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
            if (!input.Method.Equals(IDENT_METHOD) && !input.Method.Equals(GET_CPU_METHOD))
            {
                // Creation d'une nouvelle task
                Tasks.Add(new Tuple<int, NodeState>(input.NodeTaskId, NodeState.WAIT));
                Results.Add(new Tuple<int, object>(input.NodeTaskId, null));
            }
            Object res = executor.DoWork(input);
            if (res != null) { 
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
                    Console.WriteLine("Send node info to server");
                    Thread.Sleep(3000);
                }
            };
            bw.RunWorkerAsync();
            return null;
        }

        protected void LaunchBGForWork(Action<object, DoWorkEventArgs> ProcessFunction,DataInput taskData)
        {
            BackgroundWorker bw = new BackgroundWorker();
            //// Abonnage ////
            bw.DoWork += new DoWorkEventHandler(ProcessFunction);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkerEndProcess);
            int workerTaskID = createWorkerTask(taskData.NodeTaskId);
            Tuple< DataInput,int> dataAndMeta = new Tuple<DataInput ,int>(taskData, workerTaskID);
            bw.RunWorkerAsync(dataAndMeta);
        }

        protected void WorkerStart(Tuple<DataInput, int> metaData)
        {
            updateWorkerTaskStatus(metaData.Item2, NodeState.WORK);
            if (isFirstToStart(metaData.Item1.TaskId))
            {
                DataInput resp = metaData.Item1;
                resp.MsgType = MessageType.CALL;
                resp.Method = "TASK_STATUS";
                resp.Data = new Tuple<NodeState, double>(NodeState.WORK, 0);
                SendData(Orch, resp);
            }
            else
            {

            }
            // TODO sinon le taux d'avancement du noeud
        }

        protected void WorkerEndProcess(object sender, RunWorkerCompletedEventArgs e)
        {
            // Manage if e.Error != null
            Tuple<DataInput, int> data = (Tuple<DataInput, int>)e.Result;
            DataInput resp = data.Item1;
            updateWorkerTaskStatus(data.Item2, NodeState.FINISH);
            IReducer reducer = WorkerFactory.GetWorker(resp.Method).Reducer;
            Object result = reducer.reduce(getResultFromTaskId(resp.NodeTaskId), resp.Data);
            if (TaskIsCompleted(resp.NodeTaskId,data.Item2))
            {
                resp.Data = result;
                SendData(Orch, resp);
            }
            else
            {
                updateResult(result, data.Item1.NodeTaskId);
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
            bool absent = true;
            int lastsubID = LastSubTaskID;
            for (int i = 0; i < Tasks.Count; i++)
            {
                if (Tasks[i].Item1 == taskID)
                {
                    // Ajout d'une workerTask dans WorkerTaskStatus avec le statut WORK
                    WorkerTaskStatus.Add(new Tuple<int, int, NodeState>(taskID, lastsubID, NodeState.WORK));
                    absent = false;
                }
            }
            if (absent)
            {
                throw new Exception("Aucune Task trouvé dans la liste Tasks du Node pour cet ID : " + taskID);
            }
            return lastsubID;
        }

        private void updateWorkerTaskStatus(int workerTaskID, NodeState status)
        {
            for (int i = 0; i < WorkerTaskStatus.Count; i++)
            {
                if (WorkerTaskStatus[i].Item2 == workerTaskID)
                {
                    WorkerTaskStatus[i] = new Tuple<int, int, NodeState>(WorkerTaskStatus[i].Item1, WorkerTaskStatus[i].Item2, status);
                }
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

            foreach (Tuple<int, int, NodeState> workerTask in WorkerTaskStatus)
            {
                if (workerTask.Item1 == taskID)
                {
                    nbWorking = workerTask.Item3 == NodeState.WORK ? nbWorking + 1 : nbWorking;
                }
            }

            return nbWorking == 1;
        }

        private bool TaskIsCompleted(int taskId,int currentTaskId)
        {
            bool completed = true;
            foreach(Tuple<int,int,NodeState> workerTask in WorkerTaskStatus)
            {
                if(workerTask.Item1 == taskId && workerTask.Item3 != NodeState.FINISH && workerTask.Item2 != currentTaskId)
                {
                    completed = false;
                }
            }
            return completed;
        }

        private object getResultFromTaskId(int taskId)
        {
            foreach(Tuple<int,Object> result in Results)
            {
                if(result.Item1 == taskId)
                {
                    return result.Item2;
                }
            }
            throw new Exception("Aucune ligne de résultat ne correspond à cette tâche");
        }

        private void updateResult(object result, int nodeTaskId)
        {
            for (int i = 0 ; i < Results.Count; i ++)
            {
                if (Results[i].Item1 == nodeTaskId)
                {
                    Results[i] = new Tuple<int, object>(nodeTaskId, result);
                }
            }
        }
        #endregion
    }
}
