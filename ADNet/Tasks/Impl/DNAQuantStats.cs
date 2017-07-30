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
        public IReducer<string, string> Reducer { get; set ; }
        public Action<DataInput> ProcessFunction;
        private List<BackgroundWorker> Workers;
        #endregion

        #region Ctor
        public DNAQuantStats(Action<DataInput> function, IMapper<String, String> mapper, IReducer<String, String> reducer)
        {
            Mapper = (QuantStatsMapper<String, String>) mapper;
            Reducer = (QuantStatsReducer<String, String>) reducer;
            ProcessFunction = function;
        }
        #endregion

        #region Interface Implements
        public void CancelWork()
        {
            throw new NotImplementedException();
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
            string[] items = new string[] { input };
            var pairs = items.Select((item, k) => new KVPair<int, string>() { Key = k, Value = item });

            throw new NotImplementedException();
        }


        #endregion
    }
}
