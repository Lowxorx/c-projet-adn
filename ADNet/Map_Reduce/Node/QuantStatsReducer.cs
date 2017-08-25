using NodeNet.Map_Reduce;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ADNet.Map_Reduce.Node
{
    /// <summary>
    /// Classe de Reducer implémentant la classe Abstraite AbstractReducer pour le Module 1
    /// </summary>
    public class QuantStatsReducer : AbstractReducer
    {
        /// <summary>
        /// Méthode permettant de réduire les résultats des traitements après le découpage
        /// </summary>
        /// <param name="input">Liste de résultats</param>
        /// <returns>résultats regroupés</returns>
        public override object Reduce(ConcurrentBag<object> input)
        {
            Dictionary<string, int> concat = new Dictionary<string,int>();
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
                        concat[entry.Key] = occur + entry.Value;
                    }
                    else
                    {
                        concat.Add(entry.Key,entry.Value);
                    }
                }
            }
            Tuple<Dictionary<string, int>, string, string> result = new Tuple<Dictionary<string, int>, string, string>(concat, startSeq, endSeq);
            return result;
        }

        /// <summary>
        /// Méthode permettant de recouper les caractères entre les découopages
        /// </summary>
        /// <param name="prevEndSeq">Caractères de fin de la séquence précédente</param>
        /// <param name="startSeq">Caractères de début de la séquence suivante</param>
        /// <param name="concat">Liste des résultats</param>
        private void UpdatePairsSeq(string prevEndSeq, string startSeq, Dictionary<string,int> concat)
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
                    concat[s] = occur + 1;
                }
                else if (s.Length == 4 || pairesbases.Contains(s))
                {
                    concat.Add(s, 1);
                }
            }
        }

        public override object Clone()
        {
            return new QuantStatsReducer();
        }
    }
}
