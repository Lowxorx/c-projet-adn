using NodeNet.Map_Reduce;
using System;
using System.Collections.Generic;

namespace ADNet.Map_Reduce.Node
{
    public class QuantStatsReducer : IReducer
    {
        #region Methods

        public Object reduce(List<object> input)
        {
            List<Tuple<char, int>> result = new List<Tuple<char, int>>();

            if (input.Count <= 0)
            {
                return result;
            }

            foreach (List<Tuple<char, int>> inputpl in input)
            {
                foreach (Tuple<char,int> t in inputpl)
                {
                    bool present = false;
                    for (int i = 0; i < result.Count; i++)
                    {
                        if (result[i].Item1 == t.Item1)
                        {
                            present = true;
                            result[i] = new Tuple<char, int>(result[i].Item1, result[i].Item2 + t.Item2);
                        }
                    }
                    if (!present)
                    {
                        result.Add(t);
                    }
                }
            }
            return result;
        }
        #endregion
        public object Clone()
        {
            return new QuantStatsReducer();
        }
    }
}
