using System;
using NodeNet.Map_Reduce;
using NodeNet.Tasks;
using NodeNet.Data;
using c_projet_adn.Map_Reduce.Impl;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using NodeNet.Misc;
using ADNet.Map_Reduce.Impl;
using NodeNet.Network.Nodes;

namespace c_projet_adn.Tasks.Impl
{
    public class DNAQuantStats : GenericTaskExecutor<List<String>, String, List<Tuple<char, int>>>
    {
        #region Properties
        public override IMapper<List<String>, String> Mapper { get; set; }
        public override IReducer<List<Tuple<char, int>>, List<Tuple<char, int>>> Reducer { get; set; }

        public Action<DataInput> ProcessFunction;
        
        #endregion

        #region Ctor
        public DNAQuantStats(Node node, Action<DataInput> function, IMapper<List<String>, String> mapper, IReducer<List<Tuple<char, int>>, List<Tuple<char, int>>> reducer):base(node)
        {
            Mapper = mapper;
            Reducer = reducer;
            ProcessFunction = function;
        }
        #endregion

        #region Interface Implements
        public override void CancelWork()
        {
            
        }


        public override void ClientWork(DataInput input)
        {
            ProcessFunction(input);
        }

        public override void OrchWork(DataInput input)
        {
            ProcessFunction(input);
        }

        public override List<String> NodeWork(string input)
        {
            List<String> list = Mapper.map(input);
            foreach (string s in list)
            {
                BackgroundWorker bw = new BackgroundWorker();
                //// Abonnage ////
                bw.DoWork += new DoWorkEventHandler(BackgroundWorker_DoWork);
                bw.ProgressChanged += new ProgressChangedEventHandler(Backgroundworker_ProgressChanged);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Backgroundworker_RunWorkerCompleted);

                //// Demarrage ////
                bw.RunWorkerAsync(s);

                //// A terminer ////
            }
            return list;
        }

        public void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            base.Backgroundworker_DoWork(sender, e);
            //// Traitement métier ici ////
            string s = (string)e.Result;
            var pairs = s.Select((item, k) => new KVPair<int, string>() { Key = k, Value = s });
            //// A terminer ///////////////

        }

        public void CheckWorkers()
        {
        }

        public override object Clone()
        {
            return new DNAQuantStats(base.executor, ProcessFunction, new QuantStatsMapper(), new QuantStatsReducer());
        }
        #endregion

    }
}
