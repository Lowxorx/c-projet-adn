using ADNet.Map_Reduce.Orch;
using NodeNet.Network.Orch;
using NodeNet.Tasks;

namespace ADNet.Network.Impl
{
    /// <summary>
    /// Implémentation de la Classe Orchestrateur
    /// </summary>
    public class DnaOrchestra : Orchestrator
    {
        /// <summary>
        /// Nom de la méthode métier pour le Module 1
        /// </summary>
        private const string DnaQuantMethod = "DNA_QUANT";

        /// <summary>
        /// Connstructeur initialisant les Mapper et les Reducer nécessaires aux traitements métiers des différents modules
        /// </summary>
        /// <param name="name">Nom de cet orchestrateur</param>
        /// <param name="address">Adresse IP</param>
        /// <param name="port">Port d'écoute</param>
        public DnaOrchestra(string name, string address, int port) : base(name, address, port)
        {
            OrchQuantStatMapper quantStatMapper = new OrchQuantStatMapper();
            OrchQuantStatReducer quantStatReducer = new OrchQuantStatReducer();
            WorkerFactory.AddWorker(DnaQuantMethod, new TaskExecutor(this, ProcessMapReduce, quantStatMapper, quantStatReducer));
        }
    }
}
