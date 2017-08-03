using NodeNet.Map_Reduce;
using System;
using System.Collections.Generic;

namespace c_projet_adn.Map_Reduce.Impl
{
    public class QuantStatsMapper : IMapper<List<String>, String>
    {
        #region Properties

        #endregion

        #region Ctor
        #endregion

        #region Methods

        public List<string> map(string input)
        {
            List<string> list = new List<string>();

            foreach(char c in input.Split('\t')[3].ToCharArray())
            {
                list.Add(c.ToString());
            }
            return list;
        }


        #endregion
    }   
}
