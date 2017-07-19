using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet.Tools
{
    class StateTools
    {
        private static PerformanceCounter cpucounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        public static string GetCPU()
        {
            double cpu = Math.Round(cpucounter.NextValue());
            string currentcpuusage = cpu.ToString() + "%";
            return currentcpuusage;
        }
    }
}
