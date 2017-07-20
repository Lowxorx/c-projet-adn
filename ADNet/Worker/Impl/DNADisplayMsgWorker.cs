using System;
using NodeNet.Network;
using NodeNet.Data;
using NodeNet.Worker;
using NodeNet.Worker.Impl;

namespace ADNet.Worker.Impl
{
    public class DNADisplayMsgWorker :  IWorker<String, String>
    {
        public IMapper<String, String> mapper { get ; set; }
        public IReducer<String, String> reducer { get ; set; }

        public Action<String> ProcessFunction;

        public DNADisplayMsgWorker(Action<String> function)
        {
            ProcessFunction = function;
        }

        public void CancelWork()
        {
            throw new NotImplementedException();
        }

        public String DoWork(String input)
        {
            return input;
        }

        public void ProcessResponse(string input)
        {
            ProcessFunction(input);
        }
    }
}
