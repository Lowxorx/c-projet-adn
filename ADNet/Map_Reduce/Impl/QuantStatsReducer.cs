using NodeNet.Map_Reduce;
using NodeNet.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace c_projet_adn.Map_Reduce.Impl
{
    public class QuantStatsReducer : IReducer<List<Tuple<char, int>>, List<Tuple<char, int>>>
    {

        #region Methods

        public List<Tuple<char, int>> reduce(List<Tuple<char, int>> concat, List<Tuple<char, int>> input)
        {
            foreach(Tuple<char, int> inputpl in input)
            {
                bool present = false;
                for(int i = 0; i < concat.Count;i++)
                {
                    if(concat[i].Item1 == inputpl.Item1)
                    {
                        present = true;
                        concat[i] = new Tuple<char, int>(concat[i].Item1, concat[i].Item2+inputpl.Item2);
                    }
                 }
                if (!present)
                    concat.Add(inputpl);                
            }

            return concat;
        }
        #endregion

    }
}
