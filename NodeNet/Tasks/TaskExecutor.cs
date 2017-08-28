using System;
using NodeNet.Data;
using NodeNet.Map_Reduce;
using NodeNet.Network.Nodes;

namespace NodeNet.Tasks
{
    /// <summary>
    /// Objet de traitement de méthodes métiers ou infrastructures
    /// </summary>
    public class TaskExecutor : ITaskExecutor
    {
        /// <summary>
        /// Noeud possesseur
        /// </summary>
        protected Node Executor { get; set; }

        /// <summary>
        /// Nombre de TaskExecutors
        /// </summary>
        protected int NbWorkers = 0;
        /// <summary>
        /// Nombre de TaskExecutors terminés
        /// </summary>
        protected int NbWorkersDone = 0;

        /// <summary>
        /// Mapper utilisé
        /// </summary>
        public IMapper Mapper { get; set; }
        /// <summary>
        /// Reducer utilisé
        /// </summary>
        public IReducer Reducer { get; set; }

        /// <summary>
        /// Méthode à exécuter
        /// </summary>
        public Func<DataInput,object> ProcessAction;

        /// <summary>
        /// Constructeur initialisant le Noeud parent, la méthode à exécuter, le mapper et le reducer à utiliser
        /// </summary>
        /// <param name="node">Noeud parent</param>
        /// <param name="function">méthode à exécuter</param>
        /// <param name="mapper">Mapper/param>
        /// <param name="reducer">Reducer</param>
        public TaskExecutor(Node node, Func<DataInput, object> function,IMapper mapper,IReducer reducer)
        {
            Executor = node;
            Mapper = mapper;
            Reducer = reducer;
            ProcessAction = function;
        }

        /// <summary>
        /// Dirige l'objet de transfert reçu vers ProcessAction
        /// </summary>
        /// <param name="input">Objet de transfert</param>
        /// <returns></returns>
        public object DoWork(DataInput input)
        {
            return ProcessAction(input);
        }

        /// <summary>
        /// Réplique cet objet
        /// </summary>
        /// <returns>CLone de cette instance</returns>
        public object Clone()
        {
            IMapper newMapper = null;
            IReducer newReducer = null;
            if (Mapper != null && Reducer != null)
            {
                newMapper = (IMapper)Mapper.Clone();
                newReducer = (IReducer)Reducer.Clone();
            }
            return new TaskExecutor(Executor,ProcessAction, newMapper, newReducer);
        }
    }
}
