using NodeNet.Map_Reduce;
using System;


namespace ADNet.Map_Reduce.Orch
{
    public class OrchQuantStatMapper : IMapper
    {
        public int nbrlines { get; set; }
        public int currentline { get; set; }

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
            if(currentline >= lines.Length)
            {
                return null;
            }
            else
            {
                for(int i = currentline; i < lines.Length && i < startLine + nbrlines; i++)
                {
                    output += lines[i];
                    currentline++;
                }
                return output;
            }
        }
    }
}
