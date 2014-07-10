using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Shard.Util;

namespace Shard.Storage
{
    class ObjectWriter
    {
        public static string ComputeId(Blob blob)
        {
            using (var md = new MessageDigest())
            {
                byte[] data = Encoding.Default.GetBytes(string.Format("blob {0}\0", blob.Size));

                md.Update(data);
                md.Update(blob.Data);

                var digest = md.Digest();

                return Helper.ByteArrayToId(digest);
            }
        }

        private static Random _random = new Random();

        public static async Task<ObjectWriteResult> CreateTempFile(ObjectType type, int length, byte[] data, string rootPath)
        {
            var name = Path.Combine(rootPath, "tmp" + _random.Next(0, int.MaxValue));
            
            using (var file = File.OpenWrite(name))
            using (var stream = CompressionStream.CompressStream(file))
            using (var md = new MessageDigest())
            {
                var header = Encoding.ASCII.GetBytes(string.Format("{0} {1}\0", type.ToTypeCode(), length));

                await stream.WriteAsync(header, 0, header.Length);
                await stream.WriteAsync(data, 0, data.Length);

                md.Update(header);
                md.Update(data);

                var digest = md.Digest();

                return new ObjectWriteResult
                {
                    FileInfo = new FileInfo(name),
                    ObjectId = Helper.ByteArrayToId(digest)
                };
            }
        }

        internal class ObjectWriteResult
        {
            public string ObjectId { get; set; }
            public FileInfo FileInfo { get; set; }
        }
    }

    static class ObjectTypeExtensions
    {
        public static string ToTypeCode(this ObjectType type)
        {
            switch (type)
            {
                case ObjectType.Blob:
                    return "blob";
                case ObjectType.Commit:
                    return "commit";
                case ObjectType.Tag:
                    return "tag";
                case ObjectType.Tree:
                    return "tree";

                default:
                    throw new NotSupportedException();
            }
        }
    }
}