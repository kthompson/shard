using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Shard.Storage
{
    class RefStorage : IDisposable
    {
        public string RefsLocation { get; private set; }
        public string HeadsLocation { get; private set; }
        public string RemotesLocation { get; private set; }
        public string TagsLocation { get; private set; }

        private readonly FileSystemWatcher _watcher;

        public RefStorage(string location)
        {
            this.RefsLocation = Path.Combine(location, Ref.Refs);

            this.HeadsLocation = Path.Combine(this.RefsLocation, Ref.Heads);
            this.RemotesLocation = Path.Combine(this.RefsLocation, Ref.Remotes);
            this.TagsLocation = Path.Combine(this.RefsLocation, Ref.Tags);

            //.git/refs
            Helper.CreateDirectory(this.RefsLocation);
            //.git/refs/heads
            Helper.CreateDirectory(this.HeadsLocation);
            //.git/refs/tags
            Helper.CreateDirectory(this.TagsLocation);
            //.git/refs/remotes
            Helper.CreateDirectory(this.RemotesLocation);

            this._watcher = new FileSystemWatcher(location)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
            };

            this._watcher.Created += FileChanges;
            this._watcher.Deleted += FileChanges;

            Refresh();
        }

        public void Refresh()
        {
            this.Remotes = RefsFromPath(RefType.Remote, this.RemotesLocation);
            this.Branches = RefsFromPath(RefType.Head, this.HeadsLocation);
            this.Tags = RefsFromPath(RefType.Tag, this.TagsLocation);
        }

        private void FileChanges(object sender, FileSystemEventArgs e)
        {
            var relPath = Helper.MakeRelativePath(e.FullPath, RefsLocation);

            if (relPath.StartsWith(Ref.Heads))
            {
                UpdateCollection(this.Branches, e);
            }
            else if (relPath.StartsWith(Ref.Tags))
            {
                UpdateCollection(this.Tags, e);
            }
            else if (relPath.StartsWith(Ref.Remotes))
            {
                UpdateCollection(this.Remotes, e);
            }
        }

        private void UpdateCollection(IRefCollection collection, FileSystemEventArgs e)
        {
            var refName = Helper.MakeRelativePath(e.FullPath, RefsLocation);
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                {
                    collection.Add(Ref.FromLocation(e.FullPath, RefsLocation));
                    break;
                }
                case WatcherChangeTypes.Deleted:
                {
                    collection.Remove(refName);
                    break;
                }
            }
        }

        public IRefCollection Remotes { get; private set; }
        public IRefCollection Branches { get; private set; }
        public IRefCollection Tags { get; private set; }

        private static IRefCollection RefsFromPath(RefType type, string location)
        {
            var refsLocation = Directory.GetParent(location).FullName;
            var initialRefs = Helper.GetLocations(location)
                .Select(path => Ref.FromLocation(path, refsLocation));

            return new RefCollection(type, initialRefs);
        }

        public static string GetRefName(string refsLocations, string location)
        {
            var p = Helper.MakeRelativePath(refsLocations, location);
            return p.Replace('\\', '/');
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

            this._watcher.Dispose();
        }
    }

    interface IRefCollection : ICollection<Ref>
    {
        Ref this[string refName] { get; }
        void Remove(string refName);
    }

    class RefCollection : CustomActionCollection<Ref>, IRefCollection
    {
        private readonly RefType _type;

        public RefCollection(RefType type,  IEnumerable<Ref> initial)
            : base(null, null)
        {
            _type = type;
            this.InternalList.AddRange(initial);
        }

        protected override void OnItemAdded(Ref item)
        {
            base.OnItemAdded(item);
            item.Type = _type;

            if (!File.Exists(item.Location))
                item.Save();
        }

        protected override void OnItemRemoved(Ref item)
        {
            base.OnItemRemoved(item);

            if (File.Exists(item.Location))
                item.Delete();
        }

        public Ref this[string refName]
        {
            get
            {
                return this.FirstOrDefault(r => r.Name == refName || r.Location == refName);
            }
        }

        public void Remove(string refName)
        {
            var r = this[refName];
            if (r != null)
                this.Remove(r);
        }
    }
}