using NodeNet.Map_Reduce;
using System;
using System.Collections.Generic;

namespace ADNet.Map_Reduce.Node
{
    /// <summary>
    /// Classe Mapper côté Node pour le mapping du Module 1
    /// </summary>
    public class QuantStatsMapper : IMapper
    {
        /// <summary>
        /// Méthode de découpage de la donnée par nombre de caractères selon le nombre reçu
        /// </summary>
        /// <param name="input">objet de transfert contenant la donnée à découper</param>
        /// <param name="nbMap">Nombre de découpage</param>
        /// <returns>Liste de découpages</returns>
        public object Map(object input,int nbMap)
        {

            List<Tuple<int, char[]>> result = new List<Tuple<int, char[]>>();
            char[] data = (char[])input;
            int totalNbChar = data.Length;
            int nbCharByChunk = totalNbChar / nbMap;
            int rest = totalNbChar % nbMap;
            int currentChar = 0;

            for(int i = 0; i < nbMap; i++)
            {
                if (i == 0)
                {
                    rest += nbCharByChunk;                 
                    char[] chunk = new char[rest];
                    for(int j = 0; j < rest; j++)
                    {
                        chunk[j] = data[currentChar];
                        currentChar++;
                    }
                    result.Add(new Tuple<int, char[]>(i, chunk));
                }
                else
                {
                    char[] chunk = new char[nbCharByChunk];
                    for (int j = 0; j < nbCharByChunk; j++)
                    {
                        chunk[j] = data[currentChar];
                        currentChar++;
                    }
                    result.Add(new Tuple<int, char[]>(i, chunk));
                }
            }
            return result;
        }


        public bool MapIsEnd()
        {
            throw new NotImplementedException();
        }

        IMapper IMapper.Reset()
        {
            return this;
        }

        public object Clone()
        {
            return new QuantStatsMapper();
        }
    }   
}
