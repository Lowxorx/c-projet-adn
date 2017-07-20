using System;
using NodeNet.Tools;

namespace NodeNet.Worker.Impl
{
    class CPUStateWorker<String> : GenericWorker<String, String>
    {
        public override void CancelWork()
        {
            throw new NotImplementedException();
        }

        public override String DoWork(String input)
        {
            
            return (String)(Object)StateTools.GetCPU();
        }

        public override void ProcessResponse(String d, Func<String, String> ProcessFunction)
        {
            throw new NotImplementedException();
        }
    }
}
