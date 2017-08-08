using ADNet.Tasks.Impl;
using c_projet_adn.Tasks.Impl;
using NodeNet.Data;
using NodeNet.Network.Nodes;
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
            WorkerFactory.AddWorker(DISPLAY_MESSAGE_METHOD, new DNADisplayMsgWorker(null,null,null));
            WorkerFactory.AddWorker("QUANT", new DNAQuantStats(null,dnaQuant, null, null));
            Name = name;
            Address = address;
            Port = port;
        }


        private void dnaQuant(DataInput data)
        {
            dynamic worker = WorkerFactory.GetWorker<Object, Object, Object>(data.Method);
            List<String> list = worker.Mapper.map(worker.CastDataInput(data.Data));
            worker.Backgroundworker_DoWork(data,this);
            foreach (string s in list)
            {
                BackgroundWorker bw = new BackgroundWorker();
                //// Abonnage ////
                bw.DoWork += new DoWorkEventHandler(Backgroundworker_DoWork);
                bw.ProgressChanged += new ProgressChangedEventHandler(worker.Backgroundworker_ProgressChanged);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker.Backgroundworker_RunWorkerCompleted);

                //// Demarrage ////
                bw.RunWorkerAsync(s);

                //// A terminer ////
            }
        }

        #region EventHandlers
        protected void Backgroundworker_DoWork(object sender, DoWorkEventArgs e)
        {
            
            this.NbWorkers++;

            //DataInput input = new DataInput()
            //{
            //    Method = "TASK_STATE",
            //    NodeGUID = executor.NodeGUID,


            //};
        }
    }
}
