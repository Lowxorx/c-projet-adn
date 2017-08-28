using NodeNet.Data;
using System;

namespace NodeNet.Tasks
{
    /// <summary>
    /// Interface de la classe TaskExecutor
    /// </summary>
    public interface ITaskExecutor : ICloneable
    {
        /// <summary>
        /// Execute un traitement à partir d'un objet de transfert
        /// </summary>
        /// <param name="data">Objet de transfert</param>
        /// <returns></returns>
        object DoWork(DataInput data);
    }
}
