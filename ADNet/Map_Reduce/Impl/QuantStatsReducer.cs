using NodeNet.Map_Reduce;
using NodeNet.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace c_projet_adn.Map_Reduce.Impl
{
    public class QuantStatsReducer : IReducer
    {

        #region Methods

        public Object reduce(object concat, object input)
        {
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

    }
}
