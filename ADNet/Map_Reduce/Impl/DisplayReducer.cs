using NodeNet.Map_Reduce;
using System;


namespace ADNet.Map_Reduce.Impl
{
    public class DisplayReducer : IReducer<String, String>
    {
        public string reduce(string concat, string input)
        {
            return concat + " " + input;
        }
    }
}
