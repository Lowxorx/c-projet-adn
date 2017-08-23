using NodeNet.Map_Reduce;
using System;


namespace ADNet.Map_Reduce.Orch
{
    public class OrchQuantStatMapper : IMapper
    {
        public int NbrChunk { get; set; }
        private int CurrentChar { get; set; }
        private bool thisIsTheEnd;
        private int totalNbChar;
        private int nbCharByChunk;
        private int rest;
        private bool firstMap;

        public OrchQuantStatMapper()
        {
            CurrentChar = 0;
            firstMap = true;
        }

        public object Map(object input,int nbMap)
        {
            NbrChunk = nbMap;
            char[] sequence = (char[])input;
            char[] result;
            if (firstMap)
            {
                totalNbChar = sequence.Length;
                Console.WriteLine(@"In Orch mapping sequence size : " + sequence.Length);
                nbCharByChunk = totalNbChar / NbrChunk;
                rest = totalNbChar % NbrChunk;
                result = rest < nbCharByChunk / 2 ? new char[rest+ nbCharByChunk] : new char[rest];
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
            Console.WriteLine(@"In Orch mapping result size : " + result.Length);
            return result;
        }

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
