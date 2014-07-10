namespace Shard
{
    /// <summary>
    /// A id generator object
    /// </summary>
    public interface IKeyGenerator
    {
        /// <summary>
        /// Gets the next identifier.
        /// </summary>
        /// <returns>null if there are no more ids</returns>
        long? GetNextId();
    }
}