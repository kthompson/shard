using System;

namespace Shard
{
    /// <summary>
    /// The interface for accessing a DocumentStore
    /// </summary>
    public interface IDocumentStore : IDisposable
    {
        /// <summary>
        /// Opens a new session to the <see cref="IDocumentStore"/>.
        /// </summary>
        /// <returns></returns>
        ISession OpenSession();

        /// <summary>
        /// Initializes the <see cref="IDocumentStore"/>.
        /// </summary>
        /// <returns></returns>
        IDocumentStore Initialize();

        /// <summary>
        /// Gets the conventions used by the <see cref="IDocumentStore"/>.
        /// </summary>
        /// <value>
        /// The conventions.
        /// </value>
        IConventions Conventions { get; }
    }
}