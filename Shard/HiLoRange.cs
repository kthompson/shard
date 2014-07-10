namespace Shard
{
    /// <summary>
    /// A range of keys that can be used without repeatedly visiting the database.
    /// </summary>
    class HiLoRange : IKeyGenerator
    {
        private long _low;

        private readonly long _high;
        private readonly long _length;

        public HiLoRange(long high, long length)
        {
            this._high = high;
            this._length = length;
            this._low = 1;
        }

        public long? GetNextId()
        {
            if (this._low > _length)
                return null;

            return (this._high - 1)*this._length + this._low++;
        }
    }
}