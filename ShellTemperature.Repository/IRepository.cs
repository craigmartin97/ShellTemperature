using System.Collections.Generic;

namespace ShellTemperature.Repository
{
    public interface IRepository<T>
    {
        /// <summary>
        /// Create a new element in the repository of type T
        /// </summary>
        /// <param name="model">The model object to create in the repository</param>
        /// <returns></returns>
        bool Create(T model);

        IEnumerable<T> GetAll();
    }
}
