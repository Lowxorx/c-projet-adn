using System;
using NodeNet.Map_Reduce;
using NodeNet.Tasks;
using NodeNet.Data;

namespace c_projet_adn.Tasks.Impl
{
    public class DNAQuantStats : ITaskExecutor<String, String>
    {
        #region Properties
        public IMapper<string, string> Mapper { get; set; }
        public IReducer<string, string> Reducer { get; set ; }
        public Action<DataInput> ProcessFunction;
        #endregion

        #region Ctor
        public DNAQuantStats(Action<DataInput> function, IMapper<String, String> mapper, IReducer<String, String> reducer)
        {
            Mapper = mapper;
            Reducer = reducer;
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
            throw new NotImplementedException();
        }
        #endregion
    }
}
