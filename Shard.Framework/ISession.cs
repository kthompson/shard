using System;
using System.Collections.Generic;
using System.Linq;

namespace Shard
{
    /// <summary>
    /// An interface for communicating to the <see cref="IDocumentStore"/>
    /// </summary>
    public interface ISession : IDisposable
    {
        /// <summary>
        /// Registers the specified entity to be stored in the <see cref="IDocumentStore"/> on save.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The data.</param>
        void Store<T>(T entity);

        /// <summary>
        /// Loads the specified entity with the given identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        T Load<T>(string id);

        /// <summary>
        /// Loads the specified entity with the given identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        T Load<T>(long id);

        /// <summary>
        /// Loads the specified entities with the given identifiers.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        T[] Load<T>(params string[] ids);

        /// <summary>
        /// Loads the specified entities with the given identifiers.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids">The ids.</param>
        /// <returns></returns>
        T[] Load<T>(params long[] ids);

        /// <summary>
        /// Loads all of the entities with the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> LoadAll<T>();

        /// <summary>
        /// Deletes the specified entity from the <see cref="IDocumentStore"/> upon calling SaveChanges.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        void Delete<T>(T entity);

        /// <summary>
        /// Queries the associated <see cref="IDocumentStore"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQueryable<T> Query<T>();

        /// <summary>
        /// Applies all actions to the <see cref="IDocumentStore"/>.
        /// </summary>
        void SaveChanges();
    }
}