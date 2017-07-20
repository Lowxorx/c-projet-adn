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
        private Dictionary<object, Action<Object>> _dict = new Dictionary<object, Action<Object>>();

        public void Add<T>(T key, Action<Object> value) where T : class
        {
            _dict.Add(key, value);
        }

        public T GetValue<T>(String key) where T : class
        {
            return _dict[key] as T;
        }

        public Object GetWorker<T,R>() where T : class
        {
            return _dict.First().Key;
        }

        public Action<Object> GetMethod<T>() where T : class
        {
            return _dict.First().Value;
        }
    }
}
