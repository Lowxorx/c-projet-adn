
using ADNet.GUI.ViewModel;
using NodeNet.Network;
using NodeNet.Network.Nodes;
using System;
using System.Net.Sockets;
using NodeNet.Data;
using NodeNet.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace c_projet_adn.Network.Impl
{
    public class DNAClient : DefaultClient
    {
        private const String DNA_QUANT_METHOD = "DNA_QUANT";
        public DNAClient(String name, String adress, int port) : base(name,adress,port)
        {
            WorkerFactory.AddWorker(DNA_QUANT_METHOD, new TaskExecutor(this, DNAQuantStatDisplay, null, null));
        }

        public DNAClient(string name, string adress, int port, Socket sock) : base(name,adress,port, sock){}


        public Object ProcessDisplayMessageFunction(DataInput input)
        {
            Console.WriteLine("Client Process Display Response From Orchestrator Msg : " + input.Data);
            ViewModelLocator.VMLCliStatic.QuantDisplayResult((String)input.Data);
            return null;
        }

        public void DNAQuantStat(char[] genomicData)
        {
            
            DataInput data = new DataInput()
            {
                Data = genomicData,
                ClientGUID = NodeGUID,
                MsgType = MessageType.CALL,
                Method = DNA_QUANT_METHOD
            };
            SendData(Orch, data);
        }

        public Object DNAQuantStatDisplay(DataInput input)
        {
            Console.WriteLine("DNAQuantStatDisplay");
            String display = "";
            foreach(Tuple<string,int,double> result in (List<Tuple<string,int,double>>)input.Data)
            {
                display += result.Item1 + " : " + result.Item2.ToString() + " : " + result.Item3.ToString() + "\n"; 
            }
            ViewModelLocator.VMLCliStatic.QuantDisplayResult(display);
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
            Console.WriteLine(string.Format("read file : {0}", sw.Elapsed.TotalSeconds));
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
            Console.WriteLine("In DNA CLient sequence size : " + pairsList.Count);
            return pairsList.ToArray();
        }
    }
}