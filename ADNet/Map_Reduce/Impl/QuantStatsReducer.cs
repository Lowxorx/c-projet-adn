using NodeNet.Map_Reduce;
using System;
using System.Collections.Generic;
using System.Linq;

namespace c_projet_adn.Map_Reduce.Impl
{
    public class QuantStatsReducer<K, V> : IReducer<String, String>
    {
        #region Properties
        private IList<KVPair<K, V>> pairs = new List<KVPair<K, V>>();
        public IEnumerable<KVPair<K, V>> Pairs { get { return pairs; } }
        #endregion

        #region Ctor
        #endregion

        #region Methods
        public void AddPair(K key, V value)
        {
            pairs.Add(new KVPair<K, V>() { Key = key, Value = value});
        }

        public string reduce(string concat, string input)
        {
            throw new NotImplementedException();
        }
        private static void Reduce(string key, IList<int> values, QuantStatsReducer<string, int> context)
        {
            int total = values.Sum();
            context.AddPair(key, total);
        }
        #endregion

    }
}
