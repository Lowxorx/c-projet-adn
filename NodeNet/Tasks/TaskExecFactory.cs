using System.Collections.Concurrent;

namespace NodeNet.Tasks
{
    /// <summary>
    /// Usine à TaskExecutor
    /// </summary>
    public class TaskExecFactory : ITaskExecFactory
    {
        /// <summary>
        /// Instance singleton de la classe
        /// </summary>
        private static TaskExecFactory instance;

        /// <summary>
        /// Liste de TaskExecutor
        /// </summary>
        private static ConcurrentDictionary<string,TaskExecutor> workers;

        private TaskExecFactory(){}

        /// <summary>
        /// retourne l'instance unique actuelle de la classe
        /// </summary>
        /// <returns>Instance</returns>
        public static TaskExecFactory GetInstance()
        {
            if(instance == null)
            {
                instance = new TaskExecFactory();
                workers = new ConcurrentDictionary<string, TaskExecutor>();
            }
            return instance;
        }

        /// <summary>
        /// Ajoute un TaskExecutor à la liste de la classe
        /// </summary>
        /// <param name="methodName">Nom de la méthode du TaskExecutor</param>
        /// <param name="worker">TaskExecutor à ajouter</param>
        public void AddWorker(string methodName, TaskExecutor worker)
        { 
            workers.TryAdd(methodName, worker);
        }
        // TODO check if method name exists
        /// <summary>
        /// Vérifie si une méthode reçue existe bien parmis ses TaskExecutor
        /// </summary>
        /// <param name="methodName">Nom de la méthode</param>
        /// <returns>TaskExecutor contenant la méthode</returns>
        public TaskExecutor GetWorker(string methodName)
        {
            return (TaskExecutor)workers[methodName].Clone() ;
        }
    }
}
