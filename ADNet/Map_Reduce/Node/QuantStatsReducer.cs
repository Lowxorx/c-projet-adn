using NodeNet.Map_Reduce;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ADNet.Map_Reduce.Node
{
    public class QuantStatsReducer : IReducer
    {

        public object Reduce(ConcurrentBag<object> input)
        {

            // input = List<Tuple<int, Tuple<Dictionary<string, int>, string, string>>>

            Dictionary<string, Tuple<int, double>> concat = new Dictionary<string, Tuple<int, double>>();
            string prevEndSeq = string.Empty;
            string startSeq = string.Empty;
            string endSeq = string.Empty;
            List<Tuple<int, object>> orderedList = SortByTaskId(input);

            for (int i = 0; i < orderedList.Count; i++)
            {
                if (i == 0)
                {
                    startSeq = ((Tuple<Dictionary<string, int>, string, string>)orderedList[i].Item2).Item2;
                }
                if (i == orderedList.Count - 1)
                {
                    endSeq = ((Tuple<Dictionary<string, int>, string, string>)orderedList[i].Item2).Item3;
                }
                if (i > 0)
                {
                    UpdatePairsSeq(prevEndSeq, ((Tuple<Dictionary<string, int>, string, string>)orderedList[i].Item2).Item2, concat);
                }
                prevEndSeq = ((Tuple<Dictionary<string, int>, string, string>)orderedList[i].Item2).Item3;
            }

            foreach (Tuple<int, object> dico in orderedList)
            {
                foreach (KeyValuePair<string, int> entry in ((Tuple<Dictionary<string, int>, string, string>)dico.Item2).Item1)
                {
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
                    concat[s] = new Tuple<int, double>(occur.Item1 + 1, 0);
                }
                else if (s.Length == 4 || pairesbases.Contains(s))
                {
                    concat.Add(s, new Tuple<int, double>(1, 0));
                }
            }
        }

        public object Clone()
        {
            return new QuantStatsReducer();
        }

        public List<Tuple<int, object>> SortByTaskId(ConcurrentBag<object> input)
        {
            List<Tuple<int, object>> list = new List<Tuple<int, object>>();
            foreach (Tuple<int, object> item in input)
            {
                list.Add(item);
            }
            list.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            return list;
        }
    }
}
