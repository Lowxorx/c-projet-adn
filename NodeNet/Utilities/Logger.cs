using NodeNet.GUI.ViewModel;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace NodeNet.Utilities
{
    /// <summary>
    /// Objet permettant d'écrire les logs dans une TextBox
    /// </summary>
    public class Logger
    {
        private static readonly string AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private string LogPath;
        private static readonly VmLogBox Vmlog = ViewModelLocator.VmlLogBoxUcStatic;
        private bool isDebug;
        private bool isActive;

        public Logger(bool active)
        {
            isActive = active;
            if (!isActive) return;
#if DEBUG
            Guid id = Guid.NewGuid();
            LogPath = AppPath + @"\logs\" + id + @".log";
            Write("Logger Instance ID : " + id);
            if (!Directory.Exists(AppPath + @"\logs"))
            {
                Directory.CreateDirectory(AppPath + @"\logs");
            }
            isDebug = true;
#endif
        }

        public void Write(string value)
        {
            if (!isActive) return;
            Vmlog.LogBox += DateTime.Now.ToLongTimeString() + " - " + value + Environment.NewLine;
            if (!isDebug) return;
            try
            {
                FileMode mode = File.Exists(LogPath) ? FileMode.Append : FileMode.Create;
                string s = EncodeIso(value);
                using (StreamWriter sw = new StreamWriter(File.Open(LogPath, mode), Encoding.GetEncoding("iso-8859-1")))
                {
                    sw.WriteLine(DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + " - " + s + Environment.NewLine);
                }
            }
            catch (Exception)
            {
                //ignored
            }
        }

        public void Write(Exception e)
        {
            if (!isActive) return;
            Vmlog.LogBox += DateTime.Now.ToLongTimeString() + " - Erreur : " + e.Message + Environment.NewLine;
            if (!isDebug) return;
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
            catch (Exception)
            {
                //ignored
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
