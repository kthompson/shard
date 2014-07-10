using System;
using System.Collections.Generic;
using System.Linq;

namespace Shard
{
    /// <summary>
    /// A base implementation of a IDocumentStoreSession
    /// </summary>
    abstract class SessionBase : ISession
    {
        protected readonly List<Action> DeferredActions = new List<Action>();
        protected readonly Dictionary<string, IMetadataWrapper> TrackedObjects = new Dictionary<string, IMetadataWrapper>();
        protected readonly Dictionary<string, IKeyGenerator> KeyGenerators = new Dictionary<string, IKeyGenerator>();
        
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
            var id = this.Conventions.GetIdForEntity(entity);
            if (id == null)
            {
                var intId = this.GetFreeId<T>();
                id = this.Conventions.GetFullId<T>(intId.ToString());
                var prop = this.Conventions.GetIdProperty<T>();
                if (prop.PropertyType == typeof (long))
                {
                    this.DeferredActions.Add(() => prop.SetValue(entity, intId));
                }
                else
                {
                    this.DeferredActions.Add(() => prop.SetValue(entity, id));
                }
            }

            TrackedObjects[id] = new MetadataWrapper<T>
            {
                Id = id,
                Source = entity,
                Type = this.Conventions.GetStoragePrefix<T>()
            };
        }

        private long GetFreeId<T>()
        {
            var prefix = this.Conventions.GetStoragePrefix<T>();
            var keyGen = this.KeyGenerators.GetOrCreate(prefix, () => this.Commands.GetKeyRange(prefix));
                
            var value = keyGen.GetNextId();
            while (value == null)
            {
                keyGen = this.KeyGenerators[prefix] = this.Commands.GetKeyRange(prefix);
                value = keyGen.GetNextId();
            }

                
            return value.Value;
        }

        public virtual T Load<T>(string id)
        {
            //id = this.Conventions.GetFullId<T>(id);

            if (TrackedObjects.ContainsKey(id))
                return (T)TrackedObjects[id];

            var data = this.Commands.Load(id);
            if (data == null)
                return default(T);

            var wrapped = SerializationHelper.Deserialize<MetadataWrapper<T>>(data);
            var o = wrapped.Source;
            TrackedObjects[id] = wrapped;
            return o;
        }

        public virtual T Load<T>(long id)
        {
            return Load<T>(this.Conventions.GetFullId<T>(id.ToString()));
        }

        public virtual T[] Load<T>(params string[] ids)
        {
            return ids.Select(Load<T>).ToArray();
        }

        public virtual T[] Load<T>(params long[] ids)
        {
            return ids.Select(Load<T>).ToArray();
        }

        public IEnumerable<T> LoadAll<T>()
        {
            var typeName = this.Conventions.GetStoragePrefix<T>();

            return from id in this.Commands.TraverseRows(typeName)
                   select Load<T>(id);
        }

        public virtual void Delete<T>(T entity)
        {
            var id = this.Conventions.GetIdForEntity(entity);
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

            foreach (var kv in TrackedObjects)
            {
                var bytes = SerializationHelper.Serialize(kv.Value);
                this.Commands.Save(kv.Key, bytes);
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