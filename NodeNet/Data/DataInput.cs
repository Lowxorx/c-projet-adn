using System;

namespace NodeNet.Data
{
    /// <summary>
    /// Objet de transfert entre différent noeud
    /// </summary>
    [Serializable]
    public class DataInput
    {
        /// <summary>
        /// ID du client
        /// </summary>
        public string ClientGuid;
        /// <summary>
        /// ID du noeud
        /// </summary>
        public string NodeGuid;
        /// <summary>
        /// ID de la tâche
        /// </summary>
        public int TaskId;
        /// <summary>
        /// ID de la tâche du noeud
        /// </summary>
        public int NodeTaskId;
        /// <summary>
        /// Type du message
        /// </summary>
        public MessageType MsgType;
        /// <summary>
        /// Nom de la méthode demandée
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// Données
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// Renvoie la chaîne récapitulative des informations de l'objet de transfert
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Data -> Method : " + Method + " ClientGuid : " + ClientGuid + " NodeGuid : " + NodeGuid + " TaskId  : " + TaskId + " Data : " + Data ; 
        }
    }
}
