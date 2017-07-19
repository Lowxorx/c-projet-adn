using NodeNet.Network.Data;

namespace NodeNet.Network.Orch
{
    public interface IOrchestrator {
        void Listen();
        void SendDataToAllNodes(DataInput obj);
    }
}
