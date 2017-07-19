using System;
using NodeNet.Network;
using NodeNet.Network.Data;
using NodeNet.Worker;

namespace ADNet.Worker.Impl
{
    public class DNADisplayMsgWorker : IWorker
    {
        public void CancelWork()
        {
            throw new NotImplementedException();
        }

        public DataInput DoWork(DataInput input)
        {
            Console.WriteLine("Display message msg : " + DataFormater.Deserialize<String>(input.Data));
            return null;
        }
    }
}
