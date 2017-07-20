using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet.Data
{
    public class GenericDictionary
    {
        private Dictionary<String, object> _dict = new Dictionary<String, object>();

        public void Add<T>(String key, T value) where T : class
        {
            _dict.Add(key, value);
        }

        public T GetValue<T>(String key) where T : class
        {
            return _dict[key] as T;
        }
    }
}
