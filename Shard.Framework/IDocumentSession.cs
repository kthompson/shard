using System;
using System.Linq;

namespace Shard
{
    public interface IDocumentSession : IDisposable
    {
        void Store<T>(T data);

        T Load<T>(string id);
        T Load<T>(long id);

        T[] Load<T>(params string[] ids);
        T[] Load<T>(params long[] ids);

        void Delete<T>(T entity);

        IQueryable<T> Query<T>();

        void SaveChanges();
    }
}