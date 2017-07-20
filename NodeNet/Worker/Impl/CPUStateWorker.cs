using System;
using NodeNet.Data;
using NodeNet.Tools;

namespace NodeNet.Worker.Impl
{
    class CPUStateWorker<String> : IWorker<String, String>
    {
        public IMapper<String, String> mapper { get; set; }
        public IReducer<String, String> reducer { get; set; }

        public void CancelWork()
        {
            throw new NotImplementedException();
        }

        public String DoWork(String input)
        {
            
            return (String)(Object)StateTools.GetCPU();
        }

        public void ProcessResponse(String input)
        {
            throw new NotImplementedException();
        }
    }
}
