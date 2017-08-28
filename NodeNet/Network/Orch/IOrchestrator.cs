using NodeNet.Data;

namespace NodeNet.Network.Orch
{
    /// <summary>
    /// Interface de la classe Orchestrateur
    /// </summary>
    public interface IOrchestrator {
        void Listen();
        void SendDataToAllNodes(DataInput obj);
    }
}
