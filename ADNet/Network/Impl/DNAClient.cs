using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using ADNet.GUI.ViewModel;
using NodeNet.Data;
using NodeNet.Network;
using NodeNet.Tasks;

namespace ADNet.Network.Impl
{
    /// <summary>
    /// Implémentation de la classe DefaultClient
    /// </summary>
    public class DnaClient : DefaultClient
    {
        /// <summary>
        /// Nom de la méthode métier pour le Module 1 
        /// </summary>
        private const string DnaQuantMethod = "DNA_QUANT";

        /// <summary>
        /// Constructeur initialisant un TaskExecutor avec la méthode de traitement métier pour le Module 1
        /// </summary>
        /// <param name="name"></param>
        /// <param name="adress"></param>
        /// <param name="port"></param>
        public DnaClient(string name, string adress, int port) : base(name,adress,port)
        {
            WorkerFactory.AddWorker(DnaQuantMethod, new TaskExecutor(this, DnaQuantStatDisplay, null, null));
        }

        public DnaClient(string name, string adress, int port, Socket sock) : base(name,adress,port, sock){}


        /// <summary>
        /// Définit l'objet de transfert chargé de transmettre la requête de traitement pour le Module 1
        /// </summary>
        /// <param name="genomicData">Données à traiter</param>
        public void DnaQuantStat(char[] genomicData)
        {
            
            DataInput data = new DataInput()
            {
                Data = genomicData,
                ClientGuid = NodeGuid,
                MsgType = MessageType.Call,
                Method = DnaQuantMethod
            };
            SendData(Orch, data);
        }

        /// <summary>
        /// Affiche les résultats sur la vue à partir d'un objet de transfert contenant les données
        /// </summary>
        /// <param name="input">objet de transfert</param>
        /// <returns></returns>
        public object DnaQuantStatDisplay(DataInput input)
        {
            Console.WriteLine(@"Launch Cli");
            string display = "";
            foreach(var result in (Dictionary<string,Tuple<int,double>>)input.Data)
            {
                display += result.Key + " : " + result.Value.Item1 + " : " + result.Value.Item2.ToString(CultureInfo.InvariantCulture) + "\n"; 
            }
            ViewModelLocator.VmlCliStatic.QuantDisplayResult(display);
            return null;
        }

        /// <summary>
        /// Sélectionne les caractères correspondant aux bases au sein du fichier
        /// </summary>
        /// <param name="sourceFile">Chaîne représentant le contenu du fichier</param>
        /// <returns>tableau de caractères contenant les bases</returns>
        public char[] DnaParseData(string sourceFile)
        {
            List<char> pairsList = new List<char>();
            char[] bases = { 'A', 'G', 'T', 'C', '-' };
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //using (StreamReader sr = new StreamReader(sourceFile))
            //{
            //    while (!sr.EndOfStream)
            //    {
            //        String line = sr.ReadLine();
            //        if (line.StartsWith("#"))
            //        {
            //            foreach (char c in line)
            //            {
            //                if (bases.Contains(c))
            //                {
            //                    Console.WriteLine("Add char in list");
            //                    pairsList.Add(c);
            //                }
            //            }
            //        }
            //    }
            //}
            //sw.Stop();
            Console.WriteLine($@"read file : {sw.Elapsed.TotalSeconds}");
            foreach (string line in File.ReadAllLines(sourceFile))
            {
                if (!line.StartsWith("#"))
                {
                    foreach (char c in line)
                    {
                        if (bases.Contains(c))
                        {
                            pairsList.Add(c);
                            Console.Write(c);
                        }
                    }
                }
            }
            Console.WriteLine(@"In DNA CLient sequence size : " + pairsList.Count);
            Logger.Write("In DNA Client sequence size : " + pairsList.Count, true);
            return pairsList.ToArray();
        }
    }
}