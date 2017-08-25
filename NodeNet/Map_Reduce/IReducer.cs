using System;
using System.Collections.Concurrent;

namespace NodeNet.Map_Reduce
{
    /// <summary>
    /// Classe interface des objets Reducer
    /// </summary>
    public interface IReducer : ICloneable
    {
        /// <summary>
        /// Méthode réduisant les résultats traités après le map
        /// </summary>
        /// <param name="input">Liste d'objets concurrents traités</param>
        /// <returns></returns>
        object Reduce(ConcurrentBag<object> input);

        /// <summary>
        /// Méthode de clone de l'objet reducer
        /// </summary>
        /// <returns></returns>
        new object Clone();
    }
}
