using ADNet.Map_Reduce.Orch;
using NodeNet.Network.Orch;
using NodeNet.Tasks;

namespace ADNet.Network.Impl
{
    public class DnaOrchestra : Orchestrator
    {
        private const string DnaQuantMethod = "DNA_QUANT";
        public DnaOrchestra(string name, string address, int port) : base(name, address, port)
        {
            OrchQuantStatMapper quantStatMapper = new OrchQuantStatMapper();
            OrchQuantStatReducer quantStatReducer = new OrchQuantStatReducer();
            WorkerFactory.AddWorker(DnaQuantMethod, new TaskExecutor(this, ProcessMapReduce, quantStatMapper, quantStatReducer));
        }
    }
}
