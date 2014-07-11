using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shard.Storage;
using Shard.Util;

namespace Shard
{
    /// <summary>
    /// Basic Local Document Store
    /// </summary>
    public sealed class EmbeddedDocumentStore : IDocumentStore, ICommands
    {
        private ObjectStorage _objectStorage;
        private RefStorage _refStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedDocumentStore"/> class.
        /// </summary>
        public EmbeddedDocumentStore()
        {
            this.Conventions = new Conventions();
        }

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

        async Task ICommands.SaveAsync(string id, byte[] data)
        {
            ((ICommands)this).Delete(id);

            var sha = await this._objectStorage.WriteAsync(data);

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

        void ICommands.Save(string id, byte[] data)
        {
            ((ICommands)this).SaveAsync(id, data).Wait();
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
        /// Traverses the rows for the specified type returning the full id of the row.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        IEnumerable<string> ICommands.TraverseRows(string type)
        {
            return this._refStorage.Branches.Where(r => r.Name.StartsWith(type + "/")).Select(r => r.Name);
        }

        private object _keyLock = new object();

        IKeyGenerator ICommands.GetKeyRange(string type)
        {
            var cmds = ((ICommands)this);
            var id = "keys/" + type;

            lock (_keyLock)
            {
                var keyedIndex = GetKeyIndex(cmds, id);
                var value = keyedIndex.GetNextValue();

                SaveKeyIndex(cmds, keyedIndex, id);

                return new HiLoRange(value, 10);
            }
        }

        private void FireSaveKeyIndex(ICommands cmds, KeyIndex keyedIndex, string id)
        {
            lock (keyedIndex)
            {
                
            }
        }

        private void SaveKeyIndex(ICommands cmds, KeyIndex keyedIndex, string id)
        {
            var bytes = SerializationHelper.Serialize(keyedIndex);
            cmds.SaveAsync(id, bytes);
        }

        private readonly ConcurrentDictionary<string, KeyIndex> _keyIndices = new ConcurrentDictionary<string,KeyIndex>();
        private KeyIndex GetKeyIndex(ICommands cmds, string id)
        {
            return _keyIndices.GetOrAdd(id, s => GetKeyIndexFromDisk(cmds, s));
        }

        private static KeyIndex GetKeyIndexFromDisk(ICommands cmds, string id)
        {
            var keysBytes = cmds.Load(id);
            if (keysBytes == null)
                return new KeyIndex();

            return SerializationHelper.Deserialize<KeyIndex>(keysBytes);
        }


        class KeyIndex
        {
            public SortedSet<int> HighKeys { get; private set; }

            public KeyIndex()
            {
                this.HighKeys = new SortedSet<int>();
            }

            public int GetNextValue()
            {
                var value = GetNextHighValue(HighKeys);
                this.HighKeys.Add(value);
                return value;
            }

            private static int GetNextHighValue(ICollection<int> sortedSet)
            {
                var i = 1;
                
                foreach (var highKey in sortedSet.Where(highKey => highKey != i++))
                {
                    return highKey - 1;
                }

                return sortedSet.Count + 1;
            }
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
        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            this._refStorage.Dispose();
        }
    }
}
