using NodeNet.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet.Data
{
    public class GenericDictionary
    {
        private Dictionary<string, object> dict = new Dictionary<string, object>();

        public void Add<T>(string key, T value) where T : class
        {
            dict.Add(key, value);
        }

        public dynamic GetValue<T>(string key) where T : class
        {
            return dict[key];
        }
    }
}
