using NodeNet.Map_Reduce;
using System;


namespace ADNet.Map_Reduce.Orch
{
    /// <summary>
    /// Classe de Mapper côté orchestrateur pour le Module 1
    /// </summary>
    public class OrchQuantStatMapper : IMapper
    {
        /// <summary>
        /// Nombre de découpages
        /// </summary>
        public int NbrChunk { get; set; }
        /// <summary>
        ///  Position du caractère en cours de lecture
        /// </summary>
        private int CurrentChar { get; set; }
        /// <summary>
        /// Fin du map
        /// </summary>
        private bool thisIsTheEnd;
        /// <summary>
        /// Nombre de caractères total constituant la donnée
        /// </summary>
        private int totalNbChar;
        /// <summary>
        /// Nombre de caractères par découpage
        /// </summary>
        private int nbCharByChunk;
        /// <summary>
        /// Reste de caractères après découpage
        /// </summary>
        private int rest;
        /// <summary>
        /// Premier map
        /// </summary>
        private bool firstMap;

        /// <summary>
        /// Constructeur initialisant la position du caractère lu et le premier map
        /// </summary>
        public OrchQuantStatMapper()
        {
            CurrentChar = 0;
            firstMap = true;
        }

        /// <summary>
        /// Méthode de découpage de la données en fonction du nombre de noeuds connectés à l'orchestrateur
        /// </summary>
        /// <param name="input">Données à découper</param>
        /// <param name="nbMap">Nombre de découpages à exécuter</param>
        /// <returns></returns>
        public object Map(object input,int nbMap)
        {
            NbrChunk = nbMap;
            char[] sequence = (char[])input;
            char[] result;
            if (firstMap)
            {
                totalNbChar = sequence.Length;
                nbCharByChunk = totalNbChar / NbrChunk;
                rest = totalNbChar % NbrChunk;
                result = rest < nbCharByChunk / 2 ? new char[rest + nbCharByChunk] : new char[rest];
                firstMap = false;
            }
            else
            {
                result = new char[nbCharByChunk];
            }

            for (int i = CurrentChar, j = 0; i < totalNbChar && j < result.Length; i++, j++)
            {
                result[j] = sequence[i];
                CurrentChar++;
            }

            if (CurrentChar >= totalNbChar)
            {
                thisIsTheEnd = true;
            }
            return result;
        }

        /// <summary>
        /// Méthode de réinitialisation du mapper
        /// </summary>
        /// <returns></returns>
        public IMapper Reset()
        {
            CurrentChar = 0;
            thisIsTheEnd = false;
            totalNbChar = 0;
            nbCharByChunk = 0;
            rest = 0;
            firstMap = true;
            return this;
        }

        public bool MapIsEnd()
        {
            return thisIsTheEnd;
        }

        public object Clone()
        {
            return new OrchQuantStatMapper();
        }
    }
}
