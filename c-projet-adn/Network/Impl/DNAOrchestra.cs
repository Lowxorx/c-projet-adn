using NodeNet.Network.Iface;
using System;
namespace ADNet.impl
{
    class DNAOrchestra : IOrchestrator
    {
        public DNAOrchestra(string name, string address, int port) : base(name, address, port)
        {
        }

        public override void discoverNodes()
        {
        }

     public override void mapData(DataInput input)
        {
            throw new NotImplementedException();
        }

        public override void reduceData(DataOutput output)
        {
            throw new NotImplementedException();
        }
    }
}
