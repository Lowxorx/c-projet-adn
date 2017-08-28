using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NodeNet.Map_Reduce
{
    /// <summary>
    /// Classe abstraite implémentant IReducer
    /// </summary>
    public abstract class AbstractReducer : IReducer
    {
        public abstract object Clone();
        public abstract object Reduce(ConcurrentBag<object> input);

        /// <summary>
        /// Méthode permettant de trier les résultats reçus dans l'ordre dans lequel ils ont été découpés
        /// </summary>
        /// <param name="input">Liste de résultats</param>
        /// <returns>Liste ordonnée</returns>
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
