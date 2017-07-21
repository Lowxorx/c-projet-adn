using System;
using NodeNet.Network;
using NodeNet.Data;
using NodeNet.Worker;
using NodeNet.Worker.Impl;

namespace ADNet.Worker.Impl
{
    public class DNADisplayMsgWorker : GenericWorker<String, String>
    {
        public override IMapper<String, String> Mapper { get ; set; }
        public override IReducer<String, String> Reducer { get ; set; }

        public Action<String> ProcessFunction;

        public DNADisplayMsgWorker(Action<String> function)
        {
            ProcessFunction = function;
        }

        public override void CancelWork()
        {
            throw new NotImplementedException();
        }

        public override String DoWork(String input)
        {
            return input;
        }

        public override void ProcessResponse(string input)
        {
            ProcessFunction(input);
        }

        public override string CastData(object data)
        {
            return (String)data;
        }
    }
}
