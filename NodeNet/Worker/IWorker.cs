using NodeNet.Network.Data;
using System;


namespace NodeNet.Worker
{
    public interface IWorker
    {
        DataInput DoWork(DataInput input);

        void ProcessResponse(DataInput d, Action<DataInput> ProcessFunction);

        void CancelWork();

    }
}
