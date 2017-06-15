using NodeNet.Network.Iface;
using System;
using NodeNet.Network;

namespace ADNet.Worker.Impl
{
    public class DNAWorker1<String, Boolean> : IWorker<string, bool>
    {
        public void cancelWork()
        {
            throw new NotImplementedException();
        }

        public string doWork(bool input)
        {
            Console.WriteLine("Some work 1");
            return "Anus";
        }

        public State getState()
        {
            throw new NotImplementedException();
        }

        State IWorker<string, bool>.getState()
        {
            throw new NotImplementedException();
        }
    }
}
