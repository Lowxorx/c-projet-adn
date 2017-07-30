using NodeNet.Map_Reduce;
using System;
using System.Collections.Generic;

namespace c_projet_adn.Map_Reduce.Impl
{
    public class QuantStatsMapper<K, V> : IMapper<String, String>
    {
        #region Properties
        private IDictionary<K, IList<V>> keyvalues = new Dictionary<K, IList<V>>();
        public IDictionary<K, IList<V>> KeyValues { get { return keyvalues; } }


        #endregion

        #region Ctor
        #endregion

        #region Methods
        public void AddPair(K key, V value)
        {
            if (!keyvalues.ContainsKey(key))
                keyvalues[key] = new List<V>();

            keyvalues[key].Add(value);
        }

        public List<string> map(string input)
        {
            throw new NotImplementedException();
        }

        private static void Map(int key, string value, QuantStatsMapper<string, int> context)
        {
            context.AddPair(value, 1);
        }
        #endregion
    }   
}
