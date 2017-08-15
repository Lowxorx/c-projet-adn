using NodeNet.Map_Reduce;
using System;
using System.Collections.Generic;

namespace ADNet.Map_Reduce.Node
{
    public class QuantStatsReducer : IReducer
    {
        #region Methods

        public Object reduce(object concat, object input)
        {
            if (concat == null || ((List<Tuple<char, int>>)concat).Count == 0)
            {
                return input;
            }
            List<Tuple<char, int>> result = (List<Tuple<char, int>>)concat;
            foreach (Tuple<char, int> inputpl in (List < Tuple<char, int> > )input)
            {
                bool present = false;
                for(int i = 0; i < result.Count;i++)
                {
                    if(result[i].Item1 == inputpl.Item1)
                    {
                        present = true;
                        result[i] = new Tuple<char, int>(result[i].Item1, result[i].Item2+inputpl.Item2);
                    }
                 }
                if (!present)
                    result.Add(inputpl);                
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
