using NodeNet.Data;
using NodeNet.Network.Orch;

namespace NodeNet.Network.Nodes
{
    /// <summary>
    /// Interface de la classe Node
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// méthode de connexion
        /// </summary>
        /// <param name="address">Adresse IP</param>
        /// <param name="port">Port d'écoute</param>
        void Connect(string address, int port);
        /// <summary>
        /// Méthode d'envoi de données à un noeud
        /// </summary>
        /// <param name="node">Noeud cible</param>
        /// <param name="obj">objet de tranfert</param>
        void SendData(Node node, DataInput obj);

        /// <summary>
        /// Méthode de réception de données
        /// </summary>
        /// <param name="node">Noeud emetteur</param>
        void Receive(Node node);

        /// <summary>
        /// Méthode d'arrêt du noeud
        /// </summary>
        void Stop();

        /// <summary>
        /// Méthode d'enregistrement de l'orchestrateur auquel se connecte ce noeud
        /// </summary>
        /// <param name="orch">Orchestrateur</param>
        void RegisterOrch(Orchestrator orch);
    }
}
