using NodeNet.Network.States;
using System;

namespace NodeNet.Tasks
{
    /// <summary>
    /// Classe de tâche
    /// </summary>
    [Serializable]
    public class Task
    {
        /// <summary>
        /// ID de la tâche
        /// </summary>
        private int id;

        public int Id
        {
            get => id;
            set => id = value;
        }

        /// <summary>
        /// Etat de la tâche
        /// </summary>
        private NodeState state;
        public NodeState State
        {
            get => state;
            set => state = value;
        }

        /// <summary>
        /// Pourcentage de progression de la tâche
        /// </summary>
        private double progression;

        public double Progression
        {
            get => progression;
            set => progression = value;
        }

        /// <summary>
        /// Nom de la tâche
        /// </summary>
        private string taskName;

        public string TaskName
        {
            get => taskName;
            set => taskName = value;
        }

        /// <summary>
        /// Date de démarrage de la tâche
        /// </summary>
        private DateTime startTime;

        public DateTime StartTime
        {
            get => startTime;
            set => startTime = value;
        }

        /// <summary>
        /// Date de fin de la tâche
        /// </summary>
        private DateTime endTime;

        public DateTime EndTime
        {
            get => endTime;
            set => endTime = value;
        }
        
        /// <summary>
        /// Durée de la tâche
        /// </summary>
        private double duration;

        public double Duration
        {
            get => duration;
            set => duration = value;
        }

        /// <summary>
        /// Initialise la tâche avec un ID UNIQUE et un Etat
        /// </summary>
        /// <param name="id">ID de la tâche</param>
        /// <param name="state">Etat</param>
        public Task(int id,NodeState state)
        {
            this.id = id;
            this.state = state;
        }

        /// <summary>
        /// Initialise la tâche avec un ID UNIQUE, un Etat et un Nom de tâche
        /// </summary>
        /// <param name="id">ID de la tâche</param>
        /// <param name="state">Etat</param>
        public Task(int id, NodeState state,string taskName)
        {
            this.id = id;
            this.state = state;
            this.taskName = taskName;
        }
    }
}
