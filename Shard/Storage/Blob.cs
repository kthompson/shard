using System;

namespace Shard.Storage
{
    /// <summary>
    /// Represents the git object Blob that is used to represent files
    /// </summary>
    class Blob : AbstractObject
    {
        #region Constructors
        internal Blob(string id, long size, Func<byte[]> loader)
            : base(id)
        {
            _loader = new Lazy<byte[]>(loader);
            this.Size = size;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the data of the object.
        /// </summary>
        public byte[] Data
        {
            get { return _loader.Value; }
        }
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The SHA1 id of the object.
        /// </value>
        public override string Id
        {
            get { return base.Id ?? _id ?? (_id = ObjectWriter.ComputeId(this)); }
        }
        /// <summary>
        /// Gets the size of the blob.
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// Gets the <see cref="ObjectType"/>.
        /// </summary>
        public override ObjectType Type
        {
            get { return ObjectType.Blob; }
        }
        #endregion

        #region Private Variables
        private string _id;

        private readonly Lazy<byte[]> _loader;
        #endregion
    }
}