using NodeNet.Map_Reduce;
using System;


namespace ADNet.Map_Reduce.Orch
{
    public class OrchQuantStatMapper : IMapper
    {
        public int NbrChunk { get; set; }
        private int CurrentChar { get; set; }
        private bool ThisIsTheEnd;
        private int TotalNbChar;
        private int NbCharByChunk;
        private int Rest;
        private bool FirstMap;

        public OrchQuantStatMapper(int nbrChunk)
        {
            NbrChunk = nbrChunk;
            CurrentChar = 0;
            FirstMap = true;
        }

        public object map(object input)
        {
            char[] sequence = (char[])input;
            char[] result;
            if (FirstMap)
            {
                TotalNbChar = sequence.Length;
                NbCharByChunk = TotalNbChar / NbrChunk;
                Rest = TotalNbChar % NbrChunk;
                result = new char[Rest];
                FirstMap = false;
            }
            else
            {
                result = new char[NbCharByChunk];
            }

            for (int i = CurrentChar, j = 0; i < TotalNbChar && i < result.Length; i++, j++)
            {
                result[j] = sequence[i];
                CurrentChar++;
            }

            if (CurrentChar >= TotalNbChar)
            {
                ThisIsTheEnd = true;
            }
            return result;
        }

        public IMapper reset()
        {
            CurrentChar = 0;
            ThisIsTheEnd = false;
            TotalNbChar = 0;
            NbCharByChunk = 0;
            Rest = 0;
            FirstMap = true;
            return this;
        }

        public bool mapIsEnd()
        {
            return ThisIsTheEnd;
        }

        public object Clone()
        {
            return new OrchQuantStatMapper(NbrChunk);
        }
    }
}
