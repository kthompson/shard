using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Shard
{
    /// <summary>
    /// A base implementation of a IDocumentStoreSession
    /// </summary>
    abstract class SessionBase : ISession
    {
        protected readonly List<Action> DeferredActions = new List<Action>();
        protected readonly Dictionary<string, object> TrackedObjects = new Dictionary<string, object>();
        
        protected IConventions Conventions { get; private set; }
        protected ICommands Commands { get; private set; }
        protected IDocumentStore DocumentStore { get; private set; }

        protected SessionBase(IDocumentStore store, IConventions conventions, ICommands commands)
        {
            this.DocumentStore = store;
            this.Conventions = conventions;
            this.Commands = commands;
        }

        public virtual void Store<T>(T entity)
        {
            var id = this.Conventions.IdForEntity(entity);
            if (string.IsNullOrEmpty(id))
            {
                id = this.Conventions.GetFreeId(entity.GetType());
                var prop = this.Conventions.GetIdProperty(typeof (T));
                this.DeferredActions.Add(() => prop.SetValue(entity, id));
            }

            TrackedObjects[id] = entity;
        }

        private MetadataWrapper<T> Wrap<T>(string id, T o)
        {
            return new MetadataWrapper<T>
            {
                Id = id,
                Type = o.GetType().Name,
                Source = o,
            };
        }

        public virtual T Load<T>(string id)
        {
            id = this.Conventions.GetFullId(typeof (T), id);
            if (TrackedObjects.ContainsKey(id))
                return (T)TrackedObjects[id];

            var data = this.Commands.Load(id);
            if (data == null)
                return default(T);

            var str = Encoding.UTF8.GetString(data);
            var wrapped = JsonConvert.DeserializeObject<MetadataWrapper<T>>(str);
            var o = wrapped.Source;
            TrackedObjects[id] = o;
            return o;
        }

        public virtual T Load<T>(long id)
        {
            return Load<T>(id.ToString());
        }

        public virtual T[] Load<T>(params string[] ids)
        {
            return ids.Select(Load<T>).ToArray();
        }

        public virtual T[] Load<T>(params long[] ids)
        {
            return Load<T>(ids.Select(id => id.ToString()).ToArray());
        }

        public virtual void Delete<T>(T entity)
        {
            var id = this.Conventions.IdForEntity(entity);
            if (TrackedObjects.ContainsKey(id))
                TrackedObjects.Remove(id);

            this.DeferredActions.Add(() => this.Commands.Delete(id));
        }

        public virtual IQueryable<T> Query<T>()
        {
            throw new NotImplementedException();
        }

        public virtual void SaveChanges()
        {
            foreach (var action in DeferredActions)
                action();

            foreach (var id in TrackedObjects.Keys)
            {
                var wrapped = Wrap(id, TrackedObjects[id]);
                var data = JsonConvert.SerializeObject(wrapped);
                var bytes = Encoding.UTF8.GetBytes(data);
                this.Commands.Save(id, bytes);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.TrackedObjects.Clear();
            this.DeferredActions.Clear();
        }
    }
}