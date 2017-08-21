using NodeNet.Map_Reduce;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ADNet.Map_Reduce.Node
{
    public class QuantStatsReducer : IReducer
    {
        #region Methods

        public Object reduce(ConcurrentBag<object> input)
        {
            #region actuelle

            List<Tuple<string, int,double>> result = new List<Tuple<string, int, double>>();
            foreach (object obj in input)
                result = result.Concat((List<Tuple<string, int, double>>)obj).ToList();

            int maxA = 0;
            int maxG = 0;
            int maxT = 0;
            int maxC = 0;
            int maxAT = 0;
            int maxGC = 0;
            int maxTA = 0;
            int maxCG = 0;
            int totalunkonwbase = 0;
            List<Tuple<string, int, double>> finalresult = new List<Tuple<string, int, double>>();
            List<Tuple<string, int, double>> séquences4 = new List<Tuple<string, int, double>>();

            foreach (var kvp in result)
            {
                if (kvp.Item1.Length == 1 && kvp.Item1 != "-")
                {
                    if (kvp.Item1.Equals("A",StringComparison.InvariantCultureIgnoreCase))
                        maxA += kvp.Item2;
                    else if (kvp.Item1.Equals("G", StringComparison.InvariantCultureIgnoreCase))
                        maxG += kvp.Item2;
                    else if (kvp.Item1.Equals("T", StringComparison.InvariantCultureIgnoreCase))
                        maxT += kvp.Item2;
                    else if (kvp.Item1.Equals("C", StringComparison.InvariantCultureIgnoreCase))
                        maxC += kvp.Item2;
                }
                else if (kvp.Item1.Length == 2)
                {
                    if (kvp.Item1.Equals("AT", StringComparison.InvariantCultureIgnoreCase))
                        maxAT += kvp.Item2;
                    else if (kvp.Item1.Equals("GC", StringComparison.InvariantCultureIgnoreCase))
                        maxGC += kvp.Item2;
                    else if (kvp.Item1.Equals("TA", StringComparison.InvariantCultureIgnoreCase))
                        maxTA += kvp.Item2;
                    else if (kvp.Item1.Equals("CG", StringComparison.InvariantCultureIgnoreCase))
                        maxCG += kvp.Item2;
                }
                else if (kvp.Item1.Length == 4)
                    séquences4.Add(kvp);
                else if (kvp.Item1 == "-")
                    totalunkonwbase += kvp.Item2;
            }

            var grpseq =
                séquences4.SelectMany(
                    kvp => kvp.Item1.Select(
                        t => new
                        {
                            key = kvp.Item2,
                            value = kvp.Item1,
                        }))
                .GroupBy(x => x.value)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(
                        x => Tuple.Create(x.key, x.value)).ToList());

            Tuple<string, int, double> maxseq = new Tuple<string, int, double>("", 0,0.0);
            foreach (var tmp in grpseq)
            {
                if (tmp.Value.Count > maxseq.Item2)
                    maxseq = new Tuple<string, int, double>(tmp.Key, tmp.Value.Count,0.0);
            }
            int totalbases = maxA + maxC + maxG + maxT;
            finalresult.Add(new Tuple<string, int, double>("A", maxA, (double)(Decimal.Divide(maxA, totalbases)) * 100));
            finalresult.Add(new Tuple<string, int, double>("G", maxG, (double)(Decimal.Divide(maxG, totalbases)) * 100));
            finalresult.Add(new Tuple<string, int, double>("T", maxT, (double)(Decimal.Divide(maxT, totalbases)) * 100));
            finalresult.Add(new Tuple<string, int, double>("C", maxC, (double)(Decimal.Divide(maxC, totalbases)) * 100));
            finalresult.Add(new Tuple<string, int, double>("-", totalunkonwbase, 0));
            finalresult.Add(new Tuple<string, int, double>("AT", maxAT, 0));
            finalresult.Add(new Tuple<string, int, double>("GC", maxGC, 0));
            finalresult.Add(new Tuple<string, int, double>("TA", maxTA, 0));
            finalresult.Add(new Tuple<string, int, double>("CG", maxCG, 0));
            finalresult.Add(new Tuple<string, int, double>(maxseq.Item1, maxseq.Item2, 0));

            return finalresult;
            #endregion
        }
        #endregion
        public object Clone()
        {
            return new QuantStatsReducer();
        }
    }
}
