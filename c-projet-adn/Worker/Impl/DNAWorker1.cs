using NodeNet.Network.Iface;
using System;
using System.Threading.Tasks;

namespace NodeNet.impl
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
    }
}
