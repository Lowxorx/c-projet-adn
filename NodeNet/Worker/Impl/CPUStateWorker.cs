using System;
using NodeNet.Data;
using System.Diagnostics;
using System.Management;
using System.Linq;

namespace NodeNet.Worker.Impl
{
    public class CPUStateWorker : IWorker<Tuple<float, double>, Tuple<PerformanceCounter, ManagementObjectSearcher>>
    {
        public IMapper<Tuple<float, double>, Tuple<PerformanceCounter, ManagementObjectSearcher>> Mapper { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IReducer<Tuple<float, double>, Tuple<PerformanceCounter, ManagementObjectSearcher>> Reducer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private Action<Tuple<float, double>> processFunction;

        public CPUStateWorker(Action<Tuple<float, double>> func)
        {
            processFunction = func;
        }

        public void CancelWork()
        {
            throw new NotImplementedException();
        }

        public Tuple<float, double> DoWork(Tuple<PerformanceCounter, ManagementObjectSearcher> input)
        {
            float cpuCount = input.Item1.NextValue();
            var memoryValues = input.Item2.Get().Cast<ManagementObject>().Select(mo => new
            {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();
            double ramCount = 0;
            if (memoryValues != null)
            {
                ramCount = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
            }
            return new Tuple<float, double>(cpuCount, ramCount);
        }

        public void ProcessResponse(Tuple<float, double> input)
        {
            processFunction(input);
        }

        public Tuple<PerformanceCounter, ManagementObjectSearcher> CastInputData(object data)
        {
            return (Tuple<PerformanceCounter, ManagementObjectSearcher>) data;
        }

        public Tuple<float, double> CastOutputData(object data)
        {
            return (Tuple<float, double>)data;
        }
    }
}
