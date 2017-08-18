using ADNet.GUI.ViewModel;
using System;
using System.IO;
using System.Text;

namespace c_projet_adn.Utilities
{
    public class ServerLogger : TextWriter
    {
        private readonly VMOrchView vmServer;

        public ServerLogger(VMOrchView viewModel)
        {
            vmServer = viewModel;
        }

        public override void Write(char value)
        {
            base.Write(value);
            vmServer.LogBox += DateTime.Now.ToLongTimeString() + " - " + value;
        }

        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }
    }
}
