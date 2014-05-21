namespace Shard.Storage
{
    /// <summary>
    /// Used to represent the different types of Refs
    /// </summary>
    enum RefType
    {
        /// <summary>
        /// Head/Branches
        /// </summary>
        Head,
        /// <summary>
        /// Tags
        /// </summary>
        Tag,
        /// <summary>
        /// Remotes
        /// </summary>
        Remote
    }
}