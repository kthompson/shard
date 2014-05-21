using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Shard.Storage;

namespace Shard
{
    /// <summary>
    /// Basic Local Document Store
    /// </summary>
    public class EmbeddedDocumentStore : IDocumentStore, ICommands
    {
        private ObjectStorage _objectStorage;
        private RefStorage _refStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedDocumentStore"/> class.
        /// </summary>
        public EmbeddedDocumentStore()
        {
            this.Conventions = new Conventions
            {
                GetStoragePrefix = DefaultGetStoragePrefix,
                GetIdProperty = DefaultGetIdProperty,
                IdForEntity = DefaultGetId,
                GetFullId = DefaultGetFullId,
                GetFreeId = DefaultGetFreeId
            };
        }

        string DefaultGetFreeId(Type type)
        {
            // TODO: need to perform some basic checking if this id exists or not or create something more deterministic
            var storagePrefix = this.Conventions.GetStoragePrefix(type);
            return storagePrefix + Guid.NewGuid().ToString("N");
        }

        #region Default Conventions

        static string DefaultGetStoragePrefix(Type type)
        {
            return type.Name.ToLower() + "/";
        }

        static PropertyInfo DefaultGetIdProperty(Type type)
        {
            return type.GetProperty("Id") ?? type.GetProperty("ID");
        }

        string DefaultGetFullId(Type type, object id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            if (type == null)
                throw new ArgumentNullException("type");

            var storagePrefix = this.Conventions.GetStoragePrefix(type);

            if (id is int || id is long)
            {
                var lid = (long)id;
                return storagePrefix + lid;
            }

            var s = id as string;
            if (s == null)
                throw new InvalidOperationException("Id property must be string, int, or long. Found Id of type: " + id.GetType().Name);

            var sid = s;
            if (sid.Contains("/"))
                return sid;

            return storagePrefix + id;
        }

        string DefaultGetId(object entity)
        {
            var type = entity.GetType();

            var prop = this.Conventions.GetIdProperty(type);
            if (prop == null)
                return null;

            var id = prop.GetValue(entity, null);
            if (id == null)
                return null;

            return this.Conventions.GetFullId(type, id);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Opens a new session to the <see cref="IDocumentStore" />.
        /// </summary>
        /// <returns></returns>
        public ISession OpenSession()
        {
            return new Session(this, this);
        }

        /// <summary>
        /// Initializes the <see cref="IDocumentStore" />.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">You must specify a valid Path</exception>
        public IDocumentStore Initialize()
        {
            if (this.Path == null)
                throw new InvalidOperationException("You must specify a valid Path");

            string gitDirectory = System.IO.Path.Combine(this.Path, ".git");

            if (!Directory.Exists(gitDirectory))
                Directory.CreateDirectory(gitDirectory);

            var location = Helper.MakeAbsolutePath(gitDirectory);


            _objectStorage = new ObjectStorage(location);
            _refStorage = new RefStorage(location);
            return this;
        }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Gets the conventions used by the <see cref="IDocumentStore" />.
        /// </summary>
        /// <value>
        /// The conventions.
        /// </value>
        public IConventions Conventions { get; private set; }

        #endregion

        class Session : SessionBase
        {
            public Session(IDocumentStore store, ICommands commands)
                : base(store, store.Conventions, commands)
            {
            }
        }

        void ICommands.Delete(string id)
        {
            this._refStorage.Branches.Remove(id);
        }

        void ICommands.Save(string id, byte[] data)
        {
            ((ICommands)this).Delete(id);

            var sha = this._objectStorage.Write(data);
            var @ref = new Ref
            {
                Id = sha,
                Type = RefType.Head,
                Name = id,
                IsPacked = false,
                Location = System.IO.Path.Combine(this._refStorage.HeadsLocation, id.Replace('/', '\\')) //TODO windows
            };

            this._refStorage.Branches.Add(@ref);
        }

        byte[] ICommands.Load(string id)
        {
            var r = this._refStorage.Branches[id];
            if (r == null)
                return null;

            //should always be a blob for now....
            var blob = (Blob)this._objectStorage.Read(r.Id);
            return blob.Data;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            this._refStorage.Dispose();
        }
    }
}
