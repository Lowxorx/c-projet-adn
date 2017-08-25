using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        private object DnaQuantStatDisplay(DataInput input)
        {
            string display = "";
            string basesAt = string.Empty;
            string basesCg = string.Empty;
            string unknow = string.Empty;
            string pairesbases = string.Empty;
            foreach(KeyValuePair<string, Tuple<int, double>> result in (Dictionary<string,Tuple<int,double>>)input.Data)
            {
                if (result.Key.Length == 4)
                {
                    display += result.Key + " : " + result.Value.Item1 + "\n";
                }
                else if (result.Key.Length == 2)
                {
                    pairesbases += result.Key + " : " + result.Value.Item1 + "  ";
                }
                else if (result.Key.Length == 1)
                {
                    switch (result.Key)
                    {
                        case "A":
                        case "T":
                            basesAt += result.Key + " : " + result.Value.Item1 + " : " + Math.Round((decimal)result.Value.Item2, 2).ToString(CultureInfo.InvariantCulture) + @" %" + "  ";
                            break;
                        case "C":
                        case "G":
                            basesCg += result.Key + " : " + result.Value.Item1 + " : " + Math.Round((decimal)result.Value.Item2, 2).ToString(CultureInfo.InvariantCulture) + @" %" + "  ";
                            break;
                        case "-":
                            unknow += result.Key + " : " + result.Value.Item1 + " : " + Math.Round((decimal)result.Value.Item2, 2).ToString(CultureInfo.InvariantCulture) + @" %" + "  ";
                            break;
                    }
                }
            }
            display += pairesbases + "\n";
            display += basesAt + "\n";
            display += basesCg + "\n";
            display += unknow + "\n";
            ViewModelLocator.VmlCliStatic.QuantDisplayResult(display);
            return null;
        }

        public char[] DnaParseData(string sourceFile)
        {
            List<char> pairsList = new List<char>();
            char[] bases = { 'A', 'G', 'T', 'C', '-' };
            foreach (string line in File.ReadAllLines(sourceFile))
            {
                if (!line.StartsWith("#"))
                {
                    foreach (char c in line)
                    {
                        if (bases.Contains(c))
                        {
                            pairsList.Add(c);
                            //Console.Write(c);
                        }
                    }
                }
            }
            return pairsList.ToArray();
        }
    }
}