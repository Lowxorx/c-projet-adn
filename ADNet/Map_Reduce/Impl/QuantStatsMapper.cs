using NodeNet.Map_Reduce;
using System;
using System.Collections.Generic;

namespace c_projet_adn.Map_Reduce.Impl
{
    public class QuantStatsMapper : IMapper
    {
        #region Properties

        #endregion

        #region Ctor
        #endregion

        #region Methods

        public Object map(Object input)
        {
            List<string> list = new List<string>();

            foreach(char c in ((string)input).Split('\t')[3].ToCharArray())
            {
                list.Add(c.ToString());
            }
            return list;
        }


        #endregion
    }   
}
