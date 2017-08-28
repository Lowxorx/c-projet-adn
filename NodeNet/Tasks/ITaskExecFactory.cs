namespace NodeNet.Tasks
{
    /// <summary>
    /// Interface de la TaskExecutorFactory
    /// </summary>
    interface ITaskExecFactory
    {
        /// <summary>
        /// Définit une méthode à un TaskExecutor
        /// </summary>
        /// <param name="methodName">Nom de la méthode à exécuter</param>
        /// <param name="worker">TaskExecutor qui sera implémenté</param>
        void AddWorker(string methodName, TaskExecutor worker);

        /// <summary>
        /// Renvoie un TaskExecutor selon une méthode donnée
        /// </summary>
        /// <param name="methodName">Nom de la méthode donnée</param>
        /// <returns>TaskExecutor</returns>
        TaskExecutor GetWorker(string methodName);
    }
}
