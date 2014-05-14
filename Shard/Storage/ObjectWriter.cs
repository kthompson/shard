using System;
using System.IO;
using System.Text;
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

        public static FileInfo CreateTempFile(ObjectType type, int length, byte[] data, string rootPath, out string objectId)
        {
            var name = Path.Combine(rootPath, "tmp" + _random.Next(0, int.MaxValue));
            using (var file = File.OpenWrite(name))
            using (var stream = CompressionStream.CompressStream(file))
            using (var md = new MessageDigest())
            {
                var header = Encoding.ASCII.GetBytes(string.Format("blob {0}\0", length));

                stream.Write(header, 0, header.Length);
                stream.Write(data, 0, data.Length);

                md.Update(header);
                md.Update(data);

                var digest = md.Digest();
                objectId = Helper.ByteArrayToId(digest);
            }

            return new FileInfo(name);
        }
    }
}