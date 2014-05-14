namespace Shard.Storage
{
    class DeltaOffsetPackedObjectReader : DeltaPackedObjectReader
    {

        public DeltaOffsetPackedObjectReader(PackFile packFile, long objectOffset, long dataOffset, long size, long baseOffset)
            : base(packFile, objectOffset, dataOffset, size, packFile.GetObjectLoader(baseOffset))
        {
        }

        public override ObjectType RawType
        {
            get { return ObjectType.OffsetDelta; }
        }
    }
}