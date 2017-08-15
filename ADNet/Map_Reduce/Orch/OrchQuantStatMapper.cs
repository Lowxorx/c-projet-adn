using NodeNet.Map_Reduce;
using System;


namespace ADNet.Map_Reduce.Orch
{
    public class OrchQuantStatMapper : IMapper
    {
        public int nbrlines { get; set; }
        private int currentline { get; set; }
        private bool thisThisTheEnd;

        public OrchQuantStatMapper(int nbLineByNode)
        {
            nbrlines = nbLineByNode;
            currentline = 0;
        }

        public object map(object input)
        {
            int startLine = currentline;
            String[] lines = ((String)input).Split('\n');
            String output = "";
            for(int i = currentline; i < lines.Length && i < startLine + nbrlines; i++)
            {
                output += lines[i];
                currentline++;
            }
            if (currentline >= lines.Length)
            {
                thisThisTheEnd = true;
            }
            return output;
        }

        public IMapper reset()
        {
            currentline = 0;
            return this;
        }

        public bool mapIsEnd()
        {
            return thisThisTheEnd;
        }

        public object Clone()
        {
            return new OrchQuantStatMapper(nbrlines);
        }
    }
}
