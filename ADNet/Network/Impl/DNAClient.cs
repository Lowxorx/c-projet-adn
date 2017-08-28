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
        private object DnaQuantStatDisplay(DataInput input)
        {
            string display = "";
            string basesAt = string.Empty;
            string basesCg = string.Empty;
            string unknow = string.Empty;
            string pairesbases = string.Empty;
            foreach(KeyValuePair<string, Tuple<int, double>> result in (Dictionary<string,Tuple<int,double>>)input.Data)
            {
                if (result.Key.Length == 4)
                {
                    display += result.Key + " : " + result.Value.Item1 + "\n";
                }
                else if (result.Key.Length == 2)
                {
                    pairesbases += result.Key + " : " + result.Value.Item1 + "  ";
                }
                else if (result.Key.Length == 1)
                {
                    switch (result.Key)
                    {
                        case "A":
                        case "T":
                            basesAt += result.Key + " : " + result.Value.Item1 + " : " + Math.Round((decimal)result.Value.Item2, 2).ToString(CultureInfo.InvariantCulture) + @" %" + "  ";
                            break;
                        case "C":
                        case "G":
                            basesCg += result.Key + " : " + result.Value.Item1 + " : " + Math.Round((decimal)result.Value.Item2, 2).ToString(CultureInfo.InvariantCulture) + @" %" + "  ";
                            break;
                        case "-":
                            unknow += result.Key + " : " + result.Value.Item1 + " : " + Math.Round((decimal)result.Value.Item2, 2).ToString(CultureInfo.InvariantCulture) + @" %" + "  ";
                            break;
                    }
                }
            }
            display += pairesbases + "\n";
            display += basesAt + "\n";
            display += basesCg + "\n";
            display += unknow + "\n";
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
            foreach (string line in File.ReadAllLines(sourceFile))
            {
                if (!line.StartsWith("#"))
                {
                    foreach (char c in line)
                    {
                        if (bases.Contains(c))
                        {
                            pairsList.Add(c);
                        }
                    }
                }
            }
            return pairsList.ToArray();
        }
    }
}