using NodeNet.Data;
using NodeNet.Tasks;
using NodeNet.Tasks.Impl;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Net.Sockets;
using System.Threading;

namespace NodeNet.Network.Nodes
{
    public class DefaultNode : Node
    {
        public DefaultNode(String name, String adress, int port) : base(name, adress, port)
        {
            WorkerFactory = GenericTaskExecFactory.GetInstance();
            try
            {
                WorkerFactory.AddWorker("IDENT", new IdentificationTask(null));
                WorkerFactory.AddWorker("GET_CPU", new CPUStateTask(null));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public DefaultNode(string name, string adress, int port, Socket sock) : base(name, adress, port, sock)
        {

        }

        public override object ProcessInput(DataInput input, Node node)
        {
            Console.WriteLine("Process input in defualt node");
            if (input.Method == "IDENT")
            {
                return ProcessIndent(input, node);
            }
            else if (input.Method == "GET_CPU")
            {
                StartMonitoring(input);
                return null;
            }
            else
            {
                Console.WriteLine("ProcessInput for " + input.Method);
                dynamic worker = WorkerFactory.GetWorker<Object, Object>(input.Method);
                Object result = worker.NodeWork(worker.CastInputData(input.Data));
                return result;
            }
        }

        private DataInput ProcessIndent(DataInput d, Node n)
        {
            DataInput resp = new DataInput()
            {
                ClientGUID = null,
                NodeGUID = NodeGUID,
                MsgType = MessageType.RESPONSE
            };
            return resp;
        }

        public void StartMonitoring(DataInput input)
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };
            bw.DoWork += (o, a) =>
            {
                ManagementObjectSearcher wmiObject = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                Console.WriteLine("bite");
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
                        ClientGUID = input.ClientGUID,
                        NodeGUID = NodeGUID,
                        MsgType = MessageType.RESPONSE,
                        Method = "GET_CPU",
                        TaskId = input.TaskId,
                        Data = worker.CastOutputData(result)
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
