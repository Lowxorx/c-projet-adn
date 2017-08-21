using NodeNet.Data;
using NodeNet.Network.Nodes;
using NodeNet.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using ADNet.Map_Reduce.Node;
using System.Threading;
using System.Linq;

namespace ADNet.Network.Impl
{
    public class DNANode : DefaultNode
    {
        private const String DNA_QUANT_METHOD = "DNA_QUANT";
        public DNANode(String name, String address, int port) : base(name, address, port)
        {
            WorkerFactory.AddWorker(DNA_QUANT_METHOD, new TaskExecutor(this, DnaQuantStarter, new QuantStatsMapper(), new QuantStatsReducer()));
            Name = name;
            Address = address;
            Port = port;
        }
        private Object DnaQuantStarter(DataInput input)
        {
            TaskExecutor executor = WorkerFactory.GetWorker(input.Method);

            List<char[]> list = (List<char[]>)executor.Mapper.map(input.Data);
            Console.WriteLine("In DnaQuantStater list size after mapping : " + list.Count);
            foreach (char[] s in list)
            {
                Console.WriteLine("Launch Background Worker ");
                LaunchBGForWork(DnaQuantProcess, PrepareData(input, s), list.Count);
            }
            return null;
        }

        public void DnaQuantProcess(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine("In DNAQuantPRocess");
            Tuple<int, DataInput, int> dataAndMeta = (Tuple<int, DataInput, int>)e.Argument;
            //Thread.Sleep(3000 * dataAndMeta.Item1);
            // On averti l'orchestrateur que l'on commence a process
            char[] data = (char[])dataAndMeta.Item2.Data;
            int a = data.Count();
            char[] bases = { 'A', 'T', 'G', 'C' };
            string[] pairesbases = { "AT", "TA", "GC", "CG" };

            List<char> bufferpaires = new List<char>();
            List<char> buffersequences = new List<char>();
            List<string> sequences2 = new List<string>();
            List<string> sequences4 = new List<string>();
            List<string> listpairesbases = new List<string>();

            for (int i = 0; i < data.Length; i++)
            {
                if (bases.Contains(data[i]))
                {
                    listpairesbases.Add(data[i].ToString());
                    if (buffersequences.Count < 4)
                        buffersequences.Add(data[i]);
                    else
                    {
                        sequences4.Add(String.Format("{0}{1}{2}{3}", buffersequences[0], buffersequences[1], buffersequences[2], buffersequences[3]));

                        buffersequences[0] = buffersequences[1];
                        buffersequences[1] = buffersequences[2];
                        buffersequences[2] = buffersequences[3];
                        buffersequences[3] = data[i];
                    }

                    if (bufferpaires.Count < 2)
                        bufferpaires.Add(data[i]);
                    else
                    {
                        string concat = String.Format("{0}{1}", bufferpaires[0], bufferpaires[1]);

                        if (pairesbases.Contains(concat))
                            sequences2.Add(concat);

                        bufferpaires[0] = bufferpaires[1];
                        bufferpaires[1] = data[i];

                    }
                    if (i == data.Length - 1)
                    {
                        sequences4.Add(String.Format("{0}{1}{2}{3}", buffersequences[0], buffersequences[1], buffersequences[2], buffersequences[3]));
                        string concat = String.Format("{0}{1}", bufferpaires[0], bufferpaires[1]);
                        if (pairesbases.Contains(concat))
                            sequences2.Add(concat);
                        Console.WriteLine("stop");
                    }
                }
                else if (data[i] == '-')
                    listpairesbases.Add(data[i].ToString());
            }

            List<Tuple<string, int,double>> occurences2 = new List<Tuple<string, int,double>>();
            var tmp = sequences2.GroupBy(i => i);
            foreach (var cp in tmp)
                occurences2.Add(new Tuple<string, int,double>(cp.Key, cp.Count(),0.0));


            List<Tuple<string, int,double>> occurences4 = new List<Tuple<string, int,double>>();
            var cpt = sequences4.GroupBy(i => i);
            foreach (var cp in cpt)
                occurences4.Add(new Tuple<string, int,double>(cp.Key, cp.Count(),0.0));

            var alloccurences = new List<Tuple<string, int,double>>(occurences2.Concat(occurences4));

            List<Tuple<string, int,double>> occurences1 = new List<Tuple<string, int,double>>();
            var tmpp = listpairesbases.GroupBy(i => i);
            foreach (var cp in tmpp)
                occurences1.Add(new Tuple<string, int,double>(cp.Key, cp.Count(),0.0));

            var finalresult = new List<Tuple<string, int,double>>(alloccurences.Concat(occurences1));
            dataAndMeta.Item2.Data = finalresult;
            e.Result = dataAndMeta;
        }
    }
}
