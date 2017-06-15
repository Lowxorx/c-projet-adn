namespace NodeNet.Network.Iface
{
    public interface INode
    {
        void connect(string address, int port);
        void stop();
        void registerOrch(IOrchestrator orch);
    }
}
