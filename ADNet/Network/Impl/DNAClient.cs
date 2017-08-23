using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using ADNet.GUI.ViewModel;
using NodeNet.Data;
using NodeNet.Network;
using NodeNet.Tasks;

namespace ADNet.Network.Impl
{
    public class DnaClient : DefaultClient
    {
        private const string DnaQuantMethod = "DNA_QUANT";
        public DnaClient(string name, string adress, int port) : base(name,adress,port)
        {
            WorkerFactory.AddWorker(DnaQuantMethod, new TaskExecutor(this, DnaQuantStatDisplay, null, null));
        }

        public DnaClient(string name, string adress, int port, Socket sock) : base(name,adress,port, sock){}


        public object ProcessDisplayMessageFunction(DataInput input)
        {
            Console.WriteLine(@"Client Process Display Response From Orchestrator Msg : " + input.Data);
            Logger.Write("Client Process Display Response From Orchestrator Msg : " + input.Data, true);
            ViewModelLocator.VmlCliStatic.QuantDisplayResult((string)input.Data);
            return null;
        }

        public void DnaQuantStat(char[] genomicData)
        {
            
            DataInput data = new DataInput()
            {
                Data = genomicData,
                ClientGuid = NodeGuid,
                MsgType = MessageType.Call,
                Method = DnaQuantMethod
            };
            SendData(Orch, data);
        }

        public object DnaQuantStatDisplay(DataInput input)
        {
            Console.WriteLine(@"Launch Cli");
            string display = "";
            foreach(var result in (Dictionary<string,Tuple<int,double>>)input.Data)
            {
                display += result.Key + " : " + result.Value.Item1 + " : " + result.Value.Item2.ToString(CultureInfo.InvariantCulture) + "\n"; 
            }
            ViewModelLocator.VmlCliStatic.QuantDisplayResult(display);
            return null;
        }

        public char[] DnaParseData(string sourceFile)
        {
            List<char> pairsList = new List<char>();
            char[] bases = { 'A', 'G', 'T', 'C', '-' };
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //using (StreamReader sr = new StreamReader(sourceFile))
            //{
            //    while (!sr.EndOfStream)
            //    {
            //        String line = sr.ReadLine();
            //        if (line.StartsWith("#"))
            //        {
            //            foreach (char c in line)
            //            {
            //                if (bases.Contains(c))
            //                {
            //                    Console.WriteLine("Add char in list");
            //                    pairsList.Add(c);
            //                }
            //            }
            //        }
            //    }
            //}
            //sw.Stop();
            Console.WriteLine($@"read file : {sw.Elapsed.TotalSeconds}");
            foreach (string line in File.ReadAllLines(sourceFile))
            {
                if (!line.StartsWith("#"))
                {
                    foreach (char c in line)
                    {
                        if (bases.Contains(c))
                        {
                            pairsList.Add(c);
                            Console.Write(c);
                        }
                    }
                }
            }
            Console.WriteLine(@"In DNA CLient sequence size : " + pairsList.Count);
            Logger.Write("In DNA Client sequence size : " + pairsList.Count, true);
            return pairsList.ToArray();
        }
    }
}