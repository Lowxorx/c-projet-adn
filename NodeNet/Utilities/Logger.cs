using NodeNet.GUI.ViewModel;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace NodeNet.Utilities
{
    public class Logger
    {
        static string AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        static string LogPath = AppPath + @"\dnaLog.txt";
        static VMLogBox vmlog = ViewModelLocator.VMLLogBoxUcStatic;

        public void Write(string value, bool toFile)
        {
            vmlog.LogBox += DateTime.Now.ToLongTimeString() + " - " + value + Environment.NewLine;

            if (toFile)
            {
                try
                {
                    FileMode mode;
                    if (File.Exists(LogPath))
                    {
                        mode = FileMode.Append;
                    }
                    else
                    {
                        mode = FileMode.Create;
                    }
                    string s = EncodeIso(value);
                    using (var sw = new StreamWriter(File.Open(LogPath, mode), Encoding.GetEncoding("iso-8859-1")))
                    {
                        sw.WriteLine(DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + " - " + s + Environment.NewLine);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public void Write(Exception e, bool toFile)
        {
            FileMode mode;
            if (toFile)
            {
                try
                {
                    if (File.Exists(LogPath))
                    {
                        mode = FileMode.Append;
                    }
                    else
                    {
                        mode = FileMode.Create;
                    }
                    using (var sw = new StreamWriter(File.Open(LogPath, mode), Encoding.GetEncoding("iso-8859-1")))
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
