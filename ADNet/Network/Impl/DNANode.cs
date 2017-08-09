using NodeNet.Data;
using NodeNet.Network.Nodes;
using NodeNet.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ADNet.Network.Impl
{
    public class DNANode : DefaultNode
    {
        public const String DISPLAY_MESSAGE_METHOD = "DISPLAY_MSG";
        public DNANode(String name, String address, int port) : base(name, address, port)
        {
            WorkerFactory.AddWorker("QUANT", new TaskExecutor(this,dnaQuant, null, null));
            Name = name;
            Address = address;
            Port = port;
        }


        private Object dnaQuant(DataInput data)
        {
            TaskExecutor executor = WorkerFactory.GetWorker(data.Method);
            List<String> list = (List<String>)executor.Mapper.map(data.Data);
            executor.Backgroundworker_DoWork(data,this);
            foreach (string s in list)
            {
                BackgroundWorker bw = new BackgroundWorker();
                //// Abonnage ////
                bw.DoWork += new DoWorkEventHandler(Backgroundworker_DoWork);
                bw.ProgressChanged += new ProgressChangedEventHandler(executor.Backgroundworker_ProgressChanged);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(executor.Backgroundworker_RunWorkerCompleted);

                //// Demarrage ////
                bw.RunWorkerAsync(s);

                //// A terminer ////
            }
            return null;
        }

        protected void Backgroundworker_DoWork(object sender, DoWorkEventArgs e)
        {
            
            //this.NbWorkers++;

            //DataInput input = new DataInput()
            //{
            //    Method = "TASK_STATE",
            //    NodeGUID = executor.NodeGUID,


            //};
        }
    }
}
