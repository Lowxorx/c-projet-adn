using NodeNet.Map_Reduce;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace c_projet_adn.Map_Reduce.Impl
{
    public class OrchQuantStatsMapper : QuantStatsMapper
    {
        #region Properties
        public int nbrlignes { get; set; }
        public string pathfile { get; set; }
        public int currentline { get; set; }
        #endregion

        #region Ctor
        public OrchQuantStatsMapper(string pathfile)
        {
            this.pathfile = pathfile;
        }
        #endregion

        #region Methods

        public string map()
        {
            try
            {
                string nextline = File.ReadLines(pathfile).Skip(currentline).Take(nbrlignes).Aggregate((x, y) => x + y);
                currentline += nbrlignes;
                return nextline;
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e);
                return null;
            }
        }


        #endregion
    }
}
