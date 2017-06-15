namespace NodeNet.Network.Iface
{
    public interface IOrchestrator {
        void DiscoverNodes();
        void Listen();
        void Stop();
        //void mapData(DataInput input);
        //void reduceData(DataOutput output);
        void AddNode(INode node);
        void DeleteNode(INode node);

    }
}
