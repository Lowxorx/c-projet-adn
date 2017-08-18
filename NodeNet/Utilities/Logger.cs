using NodeNet.GUI.ViewModel;
using System;
using System.IO;
using System.Text;

namespace NodeNet.Utilities
{
    public class Logger : TextWriter
    {
        static VMLogBox vmlog = ViewModelLocator.VMLLogBoxUcStatic;

        public override void Write(string value)
        {
            base.Write(value);
            vmlog.LogBox += DateTime.Now.ToLongTimeString() + " - " + value + Environment.NewLine;
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
