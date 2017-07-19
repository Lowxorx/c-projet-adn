using NodeNet.Network.Orch;

namespace NodeNet.Network.Nodes
{
    public interface INode
    {
        void Connect(string address, int port);
        void SendData(Node node, object obj);
        void Receive(Node node);
        void Stop();
        void RegisterOrch(Orchestrator orch);
    }
}
