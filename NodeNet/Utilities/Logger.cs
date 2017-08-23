using NodeNet.GUI.ViewModel;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace NodeNet.Utilities
{
    public class Logger
    {
        private static readonly string AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string LogPath = AppPath + @"\dnaLog.txt";
        private static readonly VmLogBox Vmlog = ViewModelLocator.VmlLogBoxUcStatic;

        public void Write(string value, bool toFile)
        {
            Vmlog.LogBox += DateTime.Now.ToLongTimeString() + " - " + value + Environment.NewLine;

            if (!toFile) return;
            try
            {
                FileMode mode = File.Exists(LogPath) ? FileMode.Append : FileMode.Create;
                string s = EncodeIso(value);
                using (StreamWriter sw = new StreamWriter(File.Open(LogPath, mode), Encoding.GetEncoding("iso-8859-1")))
                {
                    sw.WriteLine(DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + " - " + s + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void Write(Exception e, bool toFile)
        {
            if (!toFile) return;
            try
            {
                FileMode mode = File.Exists(LogPath) ? FileMode.Append : FileMode.Create;
                using (StreamWriter sw = new StreamWriter(File.Open(LogPath, mode), Encoding.GetEncoding("iso-8859-1")))
                {
                    sw.WriteLine(DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + " - " + "-----------------------------------------------------------------------------------");
                    sw.WriteLine(DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + " - " + e.Message);
                    sw.WriteLine(DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + " - " + e.Source);
                    sw.WriteLine(DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + " - " + e.StackTrace);
                    if (e.InnerException == null)
                    {
                        sw.WriteLine(DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + " - " + "-----------------------------------------------------------------------------------");
                        return;
                    }
                    sw.WriteLine(DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + " - " + e.InnerException.Message);
                    sw.WriteLine(DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + " - " + e.InnerException.Source);
                    sw.WriteLine(DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + " - " + e.InnerException.StackTrace);
                    sw.WriteLine(DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + " - " + "-----------------------------------------------------------------------------------");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static string EncodeIso(string s)
        {
            // Encodage de la chaine de caractères en iso
            byte[] bytes = Encoding.Default.GetBytes(s);
            s = Encoding.GetEncoding("iso-8859-1").GetString(bytes);
            return s;
        }
    }
}
