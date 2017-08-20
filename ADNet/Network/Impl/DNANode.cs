using NodeNet.Data;
using NodeNet.Network.Nodes;
using NodeNet.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using ADNet.Map_Reduce.Node;
using System.Threading;

namespace ADNet.Network.Impl
{
    public class DNANode : DefaultNode
    {
        private const String DNA_QUANT_METHOD = "DNA_QUANT";
        public DNANode(String name, String address, int port) : base(name, address, port)
        {
            WorkerFactory.AddWorker(DNA_QUANT_METHOD, new TaskExecutor(this,DnaQuantStarter, new QuantStatsMapper(), new QuantStatsReducer()));
            Name = name;
            Address = address;
            Port = port;
        }
        private Object DnaQuantStarter(DataInput input)
        {
            return LaunchWork(input,DnaQuantProcess);
        }

        public void DnaQuantProcess(object sender, DoWorkEventArgs e)
        {
            
            Tuple<int,DataInput, int> dataAndMeta = (Tuple <int,DataInput, int > )e.Argument;
            Thread.Sleep(3000*dataAndMeta.Item1);
            // On averti l'orchestrateur que l'on commence a process
            String dnaSequence = (String)dataAndMeta.Item2.Data;
            // Traitement
            List<Tuple<char, int>> result = new List<Tuple<char, int>>();
            foreach(char c in dnaSequence)
            {
                if(c == 'A' || c == 'a' || c == 'C' || c == 'c' || c == 'g' || c == 'G' || c == 'T' || c == 't')
                {
                    bool present = false;
                    for(int i = 0; i < result.Count; i++)
                    {
                        if(result[i].Item1 == c)
                        {
                            result[i] = new Tuple<char,int>(c, result[i].Item2 + 1);
                            present = true;
                        }
                    }
                    if (!present)
                    {
                        result.Add(new Tuple<char, int>(c, 1));
                    }
                }
            }
            dataAndMeta.Item2.Data = result;
            e.Result = dataAndMeta;
        }
    }
}
