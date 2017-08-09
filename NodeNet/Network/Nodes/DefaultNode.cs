using NodeNet.Data;
using NodeNet.Tasks;
using System;
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
        public DefaultNode(String name, String adress, int port) : base(name, adress, port)
        {
            WorkerFactory = TaskExecFactory.GetInstance();
            try
            {
                WorkerFactory.AddWorker("IDENT", new TaskExecutor(this, ProcessIndent,null,null));
                WorkerFactory.AddWorker("GET_CPU", new TaskExecutor(this, StartMonitoring,null,null));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public DefaultNode(string name, string adress, int port, Socket sock) : base(name, adress, port, sock){}

        public override object ProcessInput(DataInput input, Node node)
        {
            Console.WriteLine("ProcessInput for " + input.Method);
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);
            if (input.Method.Equals("CPU_STATE"))
            {
                BackgroundWorker bw = new BackgroundWorker()
                {
                    WorkerSupportsCancellation = true
                };
                bw.DoWork += (o, a) =>
                {
                    executor.DoWork(input);
                };
                bw.RunWorkerAsync();
                return null;
            }
            Object result = executor.DoWork(input);
            return result;
        }

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

        public Object StartMonitoring(DataInput input)
        {
            ManagementObjectSearcher wmiObject = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            if (PerfCpu == null)
            {
                PerfCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            }
            while (true)
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
                    Method = "GET_CPU",
                    TaskId = input.TaskId,
                    Data = new Tuple<float, double>(cpuCount, ramCount)
                };
                SendData(Orch, perfInfo);
                Console.WriteLine("Send node info to server");
                Thread.Sleep(3000);
            }
        }
    }
}
