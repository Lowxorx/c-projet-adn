using NodeNet.Network.Data;
using System;
using NodeNet.Network.Data;


namespace NodeNet.Worker
{
    public interface IWorker
    {
        DataInput DoWork(DataInput input);

        void ProcessResponse(Action<DataInput> ProcessFunction);

        void CancelWork();

    }
}
