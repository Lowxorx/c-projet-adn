using System;
using NodeNet.Network;
using NodeNet.Data;
using NodeNet.Worker;
using NodeNet.Worker.Impl;

namespace ADNet.Worker.Impl
{
    public class DNADisplayMsgWorker<String> : IWorker<String, String>
    {
        public IMapper<String, String> mapper { get ; set; }
        public IReducer<String, String> reducer { get ; set; }

        public void CancelWork()
        {
            throw new NotImplementedException();
        }

        public String DoWork(String input)
        {
            throw new NotImplementedException();
        }

        public void ProcessResponse(object data, Action<Object> ProcessFunction)
        {
            throw new NotImplementedException();
        }
    }
}
