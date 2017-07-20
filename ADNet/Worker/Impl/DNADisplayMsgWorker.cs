using System;
using NodeNet.Network;
using NodeNet.Data;
using NodeNet.Worker;
using NodeNet.Worker.Impl;

namespace ADNet.Worker.Impl
{
    public class DNADisplayMsgWorker<String> : GenericWorker<String, String>
    {
        public override void CancelWork()
        {
            throw new NotImplementedException();
        }

        public override String DoWork(String input)
        {
            throw new NotImplementedException();
        }

        public override void ProcessResponse(String d, Func<String, String> ProcessFunction)
        {
            throw new NotImplementedException();
        }
    }
}
