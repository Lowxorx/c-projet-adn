using ADNet.Map_Reduce.Node;
using ADNet.Map_Reduce.Orch;
using NodeNet.Network.Orch;
using NodeNet.Tasks;
using System;


namespace ADNet.Network.Impl
{
    public class DNAOrchestra : Orchestrator
    {
        private const String DNA_QUANT_METHOD = "DNA_QUANT";
        public DNAOrchestra(string name, string address, int port) : base(name, address, port)
        {
            OrchQuantStatMapper quantStatMapper = new OrchQuantStatMapper(6);
            QuantStatsReducer quantStatReducer = new QuantStatsReducer();
            WorkerFactory.AddWorker(DNA_QUANT_METHOD, new TaskExecutor(this, ProcessMapReduce, quantStatMapper, quantStatReducer));
        }
    }
}
