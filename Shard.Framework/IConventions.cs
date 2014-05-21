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
        Func<Type, PropertyInfo> GetIdProperty { get; set; }


        /// <summary>
        /// Gets or sets the function to determine the storage prefix.
        /// </summary>
        /// <value>
        /// The get storage prefix.
        /// </value>
        Func<Type, string> GetStoragePrefix { get; set; }


        /// <summary>
        /// Gets or sets the identifier for entity.
        /// </summary>
        /// <value>
        /// The identifier for entity.
        /// </value>
        Func<object, string> IdForEntity { get; set; }

        /// <summary>
        /// Gets or sets the function to convert an identifier to its full notation including the Storage Prefix.
        /// </summary>
        /// <value>
        /// The get full identifier.
        /// </value>
        Func<Type, object, string> GetFullId { get; set; }


        /// <summary>
        /// Gets or sets the function to get an available Id for the type.
        /// </summary>
        /// <value>
        /// The get free identifier.
        /// </value>
        Func<Type, string> GetFreeId { get; set; }
    }
}