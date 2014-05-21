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
        /// Loads the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        byte[] Load(string id);
    }
}
