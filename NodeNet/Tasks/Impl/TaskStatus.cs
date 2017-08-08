using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNet.Data;
using NodeNet.Map_Reduce;
using NodeNet.Network.Nodes;

namespace NodeNet.Tasks.Impl
{
    public class TaskStatus : GenericTaskExecutor<String, String, String>
    {
        public TaskStatus(Node node) : base(node)
        {
        }

        public override IMapper<string, string> Mapper { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IReducer<string, string> Reducer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void CancelWork()
        {
            throw new NotImplementedException();
        }

        public override void ClientWork(DataInput data)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            return new TaskStatus(base.executor);
        }

        public override string NodeWork(string input)
        {
            throw new NotImplementedException();
        }

        public override void OrchWork(DataInput data)
        {
            throw new NotImplementedException();
        }
    }
}
