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
            
            List<char[]> list = (List<char[]>)executor.Mapper.map(input.Data, Environment.ProcessorCount);
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

            Dictionary<string, Tuple<int, double>> results = new Dictionary<string, Tuple<int, double>>();
            bool buffer4OK = false;
            bool buff2OK = false;

            for (int i = 0; i < data.Length; i++)
            {   
                Console.WriteLine("position : " + i);
                if (bases.Contains(data[i]))
                {
                    buffer4OK = i >= 3 ? true : false;
                    buff2OK = i >= 1 ? true : false;
                    // Ajout ou Mise à Jour base simple
                    if (results.TryGetValue(data[i].ToString(), out var occur))
                        results[data[i].ToString()] = new Tuple<int, double>(occur.Item1 + 1, 0);
                    else
                        results.Add(data[i].ToString(), new Tuple<int, double>(1, 0));
                    // Ajout ou Mise à Jour séquences de 4
                    if (!buffer4OK)
                        buffersequences.Add(data[i]);
                    else
                    {
                        Updateres(results, data[i], bufferpaires);
                    }
                    // Ajout ou Mise à Jour paires de bases
                    if (!buff2OK)
                        bufferpaires.Add(data[i]);
                    else
                    {
                        Updateres(results, data[i], bufferpaires);
                    }
                    if (i == data.Length - 1)
                    {
                        Updateres(results, data[i], bufferpaires);
                        Updateres(results, data[i], buffersequences);
                    }
                }
                // Ajout ou Mise à Jour bases inconnues
                else if (data[i] == '-')
                    if (results.TryGetValue(data[i].ToString(), out var occur))
                        results[data[i].ToString()] = new Tuple<int, double>(occur.Item1 + 1, 0);
                    else
                        results.Add(data[i].ToString(), new Tuple<int, double>(1, 0));
            }

            dataAndMeta.Item2.Data = results;
            e.Result = dataAndMeta;
        }

        private void Updateres(Dictionary<String, Tuple<int, double>> results, char a, List<char> buffer)
        {
            string seq = String.Concat(buffer);

            if (results.TryGetValue(seq, out var tpl))
            {
                results[seq] = new Tuple<int, double>(tpl.Item1 + 1, 0);
            }
            else
            {
                results.Add(seq, new Tuple<int, double>(1, 0));
            }

            if (buffer.Count == 2)
            {
                buffer[0] = buffer[1];
                buffer[1] = a;
            }
            else if (buffer.Count == 4)
            {
                buffer[0] = buffer[1];
                buffer[1] = buffer[2];
                buffer[2] = buffer[3];
                buffer[3] = a;
            }
        }
    }
}
