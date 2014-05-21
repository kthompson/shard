using System.IO;
using System.IO.Compression;

namespace Shard.Util
{
    class CompressionStream : DeflateStream
    {
        private static Stream MoveStream(Stream stream)
        {
            // HACK: we need this to get the DeflateStream to read properly
            stream.ReadByte();
            stream.ReadByte();
            return stream;
        }

        private static FileStream OpenFile(string fileLocation)
        {
            return new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        private CompressionStream(Stream stream, CompressionMode mode, bool leaveOpen)
            : base(stream, mode, leaveOpen)
        {
        }


        public static CompressionStream DecompressFile(string filePath, bool leaveOpen = false)
        {
            return DecompressStream(OpenFile(filePath), leaveOpen);
        }

        public static CompressionStream DecompressStream(Stream stream, bool leaveOpen = false)
        {
            MoveStream(stream);
            var c = new CompressionStream(stream, CompressionMode.Decompress, leaveOpen);
            return c;
        }

        public static CompressionStream CompressStream(Stream stream, bool leaveOpen = false)
        {
            // http://stackoverflow.com/a/2331025/295840
            stream.WriteByte(0x78);
            stream.WriteByte(0x01);

            return new CompressionStream(stream, CompressionMode.Compress, leaveOpen);
        }
    }
}