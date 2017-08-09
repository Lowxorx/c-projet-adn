using NodeNet.Network.Orch;
using System;


namespace ADNet.Network.Impl
{
    class DNAOrchestra : Orchestrator
    {
        public const String DISPLAY_MESSAGE_METHOD = "DISPLAY_MSG";
        public DNAOrchestra(string name, string address, int port) : base(name, address, port)
        {

        }

    }
}
