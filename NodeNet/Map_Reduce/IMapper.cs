using System;

namespace NodeNet.Map_Reduce
{
    /// <summary>
    /// Classe interface des objets Mapper
    /// </summary>
    public interface IMapper : ICloneable
    {
        /// <summary>
        /// Méthode découpant la données reçue selon un processsu métier particulier
        /// </summary>
        /// <param name="input">Objet de transfert dont on découpera la donnée</param>
        /// <param name="nbMap">Nombre de découpage à réaliser sur la donnée</param>
        /// <returns></returns>
        object Map(object input,int nbMap);

        /// <summary>
        /// Méthode déterminant si le découpage est arrivé à son terme
        /// </summary>
        /// <returns>Booléen de fin du découpage</returns>
        bool MapIsEnd();

        /// <summary>
        /// Méthode réinitialisant le Mapper
        /// </summary>
        /// <returns>Mapper réinitialisé</returns>
        IMapper Reset();

        /// <summary>
        /// Méthode de clonbe de cet objet Mapper
        /// </summary>
        /// <returns>Nouvel objet Mapper</returns>
        new object Clone();
    }
}
