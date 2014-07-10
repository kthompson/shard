using System;
using System.Reflection;

namespace Shard
{
    /// <summary>
    /// The Conventions used by the <see cref="IDocumentStore"/>.
    /// </summary>
    public interface IConventions
    {
        /// <summary>
        /// Gets or sets the Function to find the identifier property.
        /// </summary>
        /// <value>
        /// The get identifier property.
        /// </value>
        PropertyInfo GetIdProperty<T>();


        /// <summary>
        /// Gets or sets the function to determine the type name/storage prefix.
        /// </summary>
        /// <value>
        /// The get storage prefix.
        /// </value>
        string GetStoragePrefix<T>();


        /// <summary>
        /// Gets or sets the identifier for entity.
        /// </summary>
        /// <value>
        /// The identifier for entity.
        /// </value>
        string GetIdForEntity<T>(T instance);

        /// <summary>
        /// Gets or sets the function to convert an identifier to its full notation including the Storage Prefix.
        /// </summary>
        /// <value>
        /// The get full identifier.
        /// </value>
        string GetFullId<T>(string id);
    }
}