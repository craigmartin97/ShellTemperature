using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShellTemperature.Service
{
    public interface IService<T>
    {
        /// <summary>
        /// Create a new element in the repository of type T
        /// </summary>
        /// <param name="model">The model object to create in the repository</param>
        /// <returns></returns>
        Task<bool> Create(T model);

        /// <summary>
        /// Get a collection of type T
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAll();

        /// <summary>
        /// Get the item from the database
        /// </summary>
        /// <returns></returns>
        Task<T> GetItem(Guid id);

        /// <summary>
        /// Delete an object by id
        /// </summary>
        /// <param name="id">The id of the object</param>
        /// <returns>Returns true if the item was successfully deleted</returns>
        Task<bool> Delete(Guid id);

        /// <summary>
        /// Remove range of items of type T from the data store
        /// </summary>
        /// <param name="items">Items to remove</param>
        /// <returns>Returns true if the items where deleted</returns>
        Task<bool> DeleteRange(IEnumerable<T> items);

        /// <summary>
        /// Update the model item
        /// </summary>
        /// <param name="model">Model to update</param>
        /// <returns></returns>
        Task<bool> Update(T model);
    }
}