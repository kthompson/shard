namespace Shard.Storage
{
    class DeltaReferencePackedObjectReader : DeltaPackedObjectReader
    {
        public DeltaReferencePackedObjectReader(PackFile packFile, long objectOffset, long dataOffset, long size, string baseId)
            : base(packFile, objectOffset, dataOffset, size, packFile.GetObjectLoader(baseId))
        {
        }

        public override ObjectType RawType
        {
            get { return ObjectType.ReferenceDelta; }
        }
    }
}