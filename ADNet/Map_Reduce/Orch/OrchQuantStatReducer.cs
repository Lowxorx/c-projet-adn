using NodeNet.Map_Reduce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace ADNet.Map_Reduce.Orch
{
    public class OrchQuantStatReducer : IReducer
    {
        public object Clone()
        {
            return new OrchQuantStatReducer();
        }

        public object Reduce(ConcurrentBag<object> input)
        {
            int totalbases = 0;
            Dictionary<string, Tuple<int, double>> concat = new Dictionary<string, Tuple<int, double>>();

            foreach (Dictionary<string, Tuple<int, double>> dico in input)
            {
                foreach (KeyValuePair<string, Tuple<int, double>> entry in dico)
                {
                    if (entry.Key.Length == 1)
                    {
                        totalbases += entry.Value.Item1;
                    }
                    if (concat.TryGetValue(entry.Key, out var occur))
                        concat[entry.Key] = new Tuple<int, double>(occur.Item1 + entry.Value.Item1, 0);
                    else
                        concat.Add(entry.Key, new Tuple<int, double>(1, 0));
                }

            }

            // Sélectionne les clés de 4 caractères qui ne correspondent pas à une entrée du dictionaire dont le nombre d'occurences est maximal
            List<string> keys = concat.Where(y => y.Key.Length == 4).Where(z => z.Value.Item1 != concat.Max(o => z.Value.Item1)).Select(x => x.Key).ToList();


            double percent = (double)(decimal.Divide(concat["A"].Item1, totalbases)) * 100;
            concat["A"] = new Tuple<int, double>(concat["A"].Item1, percent);
            percent = (double)(decimal.Divide(concat["T"].Item1, totalbases)) * 100;
            concat["T"] = new Tuple<int, double>(concat["T"].Item1, percent);
            percent = (double)(decimal.Divide(concat["G"].Item1, totalbases)) * 100;
            concat["G"] = new Tuple<int, double>(concat["G"].Item1, percent);
            percent = (double)(decimal.Divide(concat["C"].Item1, totalbases)) * 100;
            concat["C"] = new Tuple<int, double>(concat["C"].Item1, percent);
            percent = (double)(decimal.Divide(concat["-"].Item1, totalbases)) * 100;
            concat[""] = new Tuple<int, double>(concat["-"].Item1, percent);

            foreach (string key in keys)
            {
                concat.Remove(key);
            }
            return concat;
        }
    }
}
