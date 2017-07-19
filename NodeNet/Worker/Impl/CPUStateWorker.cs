using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNet.Network.Data;
using NodeNet.Tools;

namespace NodeNet.Worker.Impl
{
    class CPUStateWorker : IWorker
    {
        public void CancelWork()
        {
            throw new NotImplementedException();
        }

        public DataInput DoWork(DataInput input)
        {
            DataInput res = new DataInput();
            res.Data = DataFormater.Serialize(StateTools.GetCPU());
            res.msgType = MessageType.RESPONSE;
            return res;
        }
    }
}
