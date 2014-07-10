using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shard
{
    /// <summary>
    /// The commands that can be used to operate on the database immediately
    /// </summary>
    public interface ICommands
    {
        /// <summary>
        /// Deletes the entity with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        void Delete(string id);

        /// <summary>
        /// Saves the specified data with the given identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="data">The data.</param>
        void Save(string id, byte[] data);

        /// <summary>
        /// Saves the specified data with the given identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="data">The data.</param>
        Task SaveAsync(string id, byte[] data);

        /// <summary>
        /// Loads the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        byte[] Load(string id);

        /// <summary>
        /// Traverses the rows for the specified type returning the full id of the row.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        IEnumerable<string> TraverseRows(string type);

        /// <summary>
        /// Gets the key range.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        IKeyGenerator GetKeyRange(string type);
    }
}
