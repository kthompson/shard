namespace Shard.Storage
{
    /// <summary>
    /// Abstract Object is used to repesent the four primary git objects: Tag, Commit, Blob, Tree
    /// </summary>
    abstract class AbstractObject
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The SHA1 id of the object.
        /// </value>
        public virtual string Id { get; protected set; }

        /// <summary>
        /// Gets the ObjectType.
        /// </summary>
        public abstract ObjectType Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractObject"/> class.
        /// </summary>
        /// <param name="id">The sha1 id.</param>
        protected AbstractObject(string id = null)
        {
            this.Id = id;
        }
    }
}