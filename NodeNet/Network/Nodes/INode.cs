using NodeNet.Data;
using NodeNet.Network.Orch;

namespace NodeNet.Network.Nodes
{
    public interface INode
    {
        void Connect(string address, int port);
        void SendData(Node node, DataInput obj);
        void Receive(Node node);
        void RegisterOrch(Orchestrator orch);
    }
}
