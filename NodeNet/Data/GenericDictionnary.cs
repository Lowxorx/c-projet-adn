using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeNet.Data
{
    public class GenericDictionary<T>
    {
        private Dictionary<object, Action<T>> _dict = new Dictionary<object, Action<T>>();

        public void Add<T>(T key, Action<T> value) where T : class
        {
            _dict.Add(key, value);
        }

        public T GetValue<T>(String key) where T : class
        {
            return _dict[key] as T;
        }

        public T GetWorker<T>() where T : class
        {
            return (T)_dict.First().Key;
        }

        public Action<T> GetMethod<T>() where T : class
        {
            return _dict.First().Value;
        }
    }
}
