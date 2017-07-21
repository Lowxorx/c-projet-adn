using System;
using NodeNet.Data;
using System.Diagnostics;

namespace NodeNet.Worker.Impl
{
    class CPUStateWorker : GenericWorker<Tuple<float, float>, Tuple<PerformanceCounter, PerformanceCounter>>
    {
        public override IMapper<Tuple<float, float>, Tuple<PerformanceCounter, PerformanceCounter>> Mapper { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override IReducer<Tuple<float, float>, Tuple<PerformanceCounter, PerformanceCounter>> Reducer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private Action<Tuple<float, float>> processFunction;

        public CPUStateWorker(Action<Tuple<float, float>> func)
        {
            processFunction = func;
        }

        public override void CancelWork()
        {
            throw new NotImplementedException();
        }

        public override Tuple<float, float> DoWork(Tuple<PerformanceCounter, PerformanceCounter> input)
        {
            float cpuCount = input.Item1.NextValue();
            float ramCount = input.Item2.NextValue();
            return new Tuple<float, float>(cpuCount, ramCount);
        }

        public override void ProcessResponse(Tuple<float, float> input)
        {
            processFunction(input);
        }

        public override Tuple<float, float> CastData(object data)
        {
            return (Tuple<float, float>)data;
        }
    }
}
