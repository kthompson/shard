using System;
using System.IO;

namespace Shard.Storage
{
    /// <summary>
    /// Object used to represent the named pointers to commits in the repo(Refs).
    /// </summary>
    sealed class Ref
    {
        /// <summary>
        /// Constant for Refs path
        /// </summary>
        public const string Refs = "refs";
        /// <summary>
        /// Constant for Heads path
        /// </summary>
        public const string Heads = "heads";
        /// <summary>
        /// Constant for Remotes path
        /// </summary>
        public const string Remotes = "remotes";
        /// <summary>
        /// Constant for Tags path
        /// </summary>
        public const string Tags = "tags";


        private static string NameFromType(RefType type)
        {
            switch (type)
            {
                case RefType.Head:
                    return Heads;
                case RefType.Remote:
                    return Remotes;
                case RefType.Tag:
                    return Tags;
                default:
                    throw new InvalidOperationException("Invalid RefType specified: " + type);
            }
        }

        /// <summary>
        /// Gets the name. ie 'master' from .git/refs/heads/master
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets the name of the remote.
        /// </summary>
        /// <value>
        /// The name of the remote.
        /// </value>
        public string RemoteName { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is packed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is packed; otherwise, <c>false</c>.
        /// </value>
        public bool IsPacked { get; set; }

        /// <summary>
        /// Gets the type of ref.
        /// </summary>
        public RefType Type { get; set; }

        /// <summary>
        /// Gets the id that the ref points to.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets the location of the ref. ie '.git/refs/heads/master'
        /// </summary>
        public string Location { get; set; }

        ///// <summary>
        ///// Gets or sets the path of the parent directory ie .git/refs/heads.
        ///// </summary>
        ///// <value>
        ///// The refs location.
        ///// </value>
        //public string RefsLocation { get; set; }

        public static Ref FromLocation(string path, string refsPath)
        {
            var newRef = new Ref
            {
                Location = path,
                Id = File.ReadAllText(path).TrimEnd()
            };

            var relPath = Helper.MakeRelativePath(refsPath, path);
            if (UpdateIfType(newRef, relPath, RefType.Tag) ||
                UpdateIfType(newRef, relPath, RefType.Remote) ||
                UpdateIfType(newRef, relPath, RefType.Head))
            {
                return newRef;
            }

            throw new ArgumentException("The location provided does not appear to be in the repository.");
        }

        private static bool UpdateIfType(Ref newRef, string relPath, RefType refType)
        {
            var type = NameFromType(refType);

            if (!relPath.StartsWith(type)) 
                return false;

            newRef.Type = refType;
            var temp = relPath.Substring(type.Length + 1).Split(new[] { '/', '\\' }, 2);

            newRef.RemoteName = temp[0];
            newRef.Name = relPath.Substring(type.Length + 1);

            return true;
        }

        public void Refresh()
        {
            if (this.IsPacked)
                return;

            this.Id = File.ReadAllText(this.Location).TrimEnd();
        }

        public void Save()
        {
            Helper.CreateDirectory(Directory.GetParent(this.Location).FullName);
            File.WriteAllText(this.Location, this.Id);
        }

        public void Delete()
        {
            File.Delete(this.Location);
        }
    }
}