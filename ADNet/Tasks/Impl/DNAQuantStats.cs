using System;
using NodeNet.Map_Reduce;
using NodeNet.Tasks;
using NodeNet.Data;
using c_projet_adn.Map_Reduce.Impl;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;

namespace c_projet_adn.Tasks.Impl
{
    public class DNAQuantStats : ITaskExecutor<String, String>
    {
        #region Properties
        public IMapper<string, string> Mapper { get; set; }
        public IReducer<string, string> Reducer { get; set; }
        public Action<DataInput> ProcessFunction;
        public List<BackgroundWorker> Workers;
        #endregion

        #region Ctor
        public DNAQuantStats(Action<DataInput> function, IMapper<String, String> mapper, IReducer<String, String> reducer)
        {
            Mapper = (QuantStatsMapper<String, String>)mapper;
            Reducer = (QuantStatsReducer<String, String>)reducer;
            ProcessFunction = function;
        }
        #endregion

        #region Interface Implements
        public void CancelWork()
        {
            foreach (BackgroundWorker worker in Workers)
            {
                worker.CancelAsync();

            }
        }

        public string CastInputData(object data)
        {
            throw new NotImplementedException();
        }

        public void ClientWork(DataInput input)
        {
            ProcessFunction(input);
        }

        public void OrchWork(DataInput input)
        {
            ProcessFunction(input);
        }

        public string NodeWork(string input)
        {
            List<string> list = Mapper.map(input);
            foreach (string s in list)
            {
                Workers.Add(new BackgroundWorker());
                //// Abonnage ////
                Workers.Last().DoWork += new DoWorkEventHandler(backgroundworker_DoWork);
                Workers.Last().ProgressChanged += new ProgressChangedEventHandler(backgroundworker_ProgressChanged);
                Workers.Last().RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundworker_RunWorkerCompleted);

                //// Demarrage ////
                Workers.Last().RunWorkerAsync(s);

                //// A terminer ////
            }
        }

        public void CheckWorkers()
        {
            if (Workers.Count == 0)
            {
                // Informer Reducer 
            }
        }
        #endregion

        #region EventHandlers
        private void backgroundworker_DoWork(object sender, DoWorkEventArgs e)
        {
            //// Traitement métier ici ////
            string s = (string)e.Result;
            var pairs = s.Select((item, k) => new KVPair<int, string>() { Key = k, Value = s });
            //// A terminer ///////////////
        }

        private void backgroundworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void backgroundworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Workers.Remove(sender as BackgroundWorker);
            CheckWorkers();
        }
        #endregion
    }
}
