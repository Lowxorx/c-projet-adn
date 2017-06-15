using NodeNet.Network.Iface;
using NodeNet.Network.Impl;
using System;
namespace ADNet.impl
{
    class DNAOrchestra : Orchestrator
    {
        public DNAOrchestra(string name, string address, int port) : base(name, address, port)
        {
        }

        public new void AddNode(INode node)
        {
            throw new NotImplementedException();
        }

        public new void DeleteNode(INode node)
        {
            throw new NotImplementedException();
        }

        public override void DiscoverNodes()
        {
            throw new NotImplementedException();
        }

        public new void Listen()
        {
            throw new NotImplementedException();
        }

        //public override void mapData(DataInput input)
        //{
        //    throw new NotImplementedException();
        //}

        //public override void reduceData(DataOutput output)
        //{
        //    throw new NotImplementedException();
        //}

        public new void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
