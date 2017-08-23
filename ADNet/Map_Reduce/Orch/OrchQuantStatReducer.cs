using NodeNet.Map_Reduce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace ADNet.Map_Reduce.Orch
{
    public class OrchQuantStatReducer : AbstractReducer
    {
        public override object Clone()
        {
            return new OrchQuantStatReducer();
        }

        public override object Reduce(ConcurrentBag<object> input)
        {
            Dictionary<string, Tuple<int, double>> concat = new Dictionary<string, Tuple<int, double>>();
            string prevEndSeq = string.Empty;
            List<Tuple<int, object>> orderedList = SortByTaskId(input);
            int totalbases = 0;
            for (int i = 0; i < orderedList.Count; i++)
            {
                if (i > 0)
                {
                    UpdatePairsSeq(prevEndSeq, ((Tuple<Dictionary<string, int>, string, string>)orderedList[i].Item2).Item2, concat);
                }
                prevEndSeq = ((Tuple<Dictionary<string, int>, string, string>)orderedList[i].Item2).Item3;
            }

            foreach (Tuple<int,object> dico in orderedList)
            {

                foreach (KeyValuePair<string, int> entry in ((Tuple<Dictionary<string, int>, string, string>)dico.Item2).Item1)
                {
                    if (entry.Key.Length == 1)
                    {
                        totalbases += entry.Value;
                    }
                    if (concat.TryGetValue(entry.Key, out var occur))
                    {
                        concat[entry.Key] = new Tuple<int, double>(occur.Item1 + entry.Value, 0);
                    }
                    else
                    {
                        concat.Add(entry.Key, new Tuple<int, double>(entry.Value, 0));
                    } 
                }
            }

            //Sélectionne les clés de 4 caractères qui ne correspondent pas à une entrée du dictionaire dont le nombre d'occurences est maximal
            List<string> keys = concat.Where(y => y.Key.Length == 4)
                .Where(z => z.Value.Item1 != concat.Max(o => z.Value.Item1))
                .Select(x => x.Key).ToList();
            foreach (string key in keys)
            {
                concat.Remove(key);
            }


            double percent = (double)(decimal.Divide(concat["A"].Item1, totalbases)) * 100;
            concat["A"] = new Tuple<int, double>(concat["A"].Item1, percent);
            percent = (double)(decimal.Divide(concat["T"].Item1, totalbases)) * 100;
            concat["T"] = new Tuple<int, double>(concat["T"].Item1, percent);
            percent = (double)(decimal.Divide(concat["G"].Item1, totalbases)) * 100;
            concat["G"] = new Tuple<int, double>(concat["G"].Item1, percent);
            percent = (double)(decimal.Divide(concat["C"].Item1, totalbases)) * 100;
            concat["C"] = new Tuple<int, double>(concat["C"].Item1, percent);
            percent = (double)(decimal.Divide(concat["-"].Item1, totalbases)) * 100;
            concat["-"] = new Tuple<int, double>(concat["-"].Item1, percent);


            return concat;
        }

        private void UpdatePairsSeq(string prevEndSeq, string startSeq, Dictionary<string, Tuple<int, double>> concat)
        {
            List<string> joinSequences = new List<string>()
            {
                prevEndSeq.Substring(2, 1) + startSeq.Substring(0,1),
                prevEndSeq + startSeq.Substring(0, 1),
                prevEndSeq.Substring(1, 2) + startSeq.Substring(0, 2),
                prevEndSeq.Substring(2, 1) + startSeq
            };

            string[] pairesbases = { "AT", "GC" };

            foreach (string s in joinSequences)
            {
                if (concat.TryGetValue(s, out var occur))
                {
                    concat[s] = new Tuple<int,double>(occur.Item1 + 1,0.0);
                }
                else if (s.Length == 4 || pairesbases.Contains(s))
                {
                    concat.Add(s, new Tuple<int,double>(1,0.0));
                }
            }
        }
    }
}
