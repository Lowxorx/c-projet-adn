using NodeNet.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet.Network.Iface
{
    public interface IOrchestrator {
        void discoverNodes();
        void listen();
        void stop();
        //void mapData(DataInput input);
        //void reduceData(DataOutput output);
        void addNode(INode node);
        void deleteNode(INode node);

    }
}
