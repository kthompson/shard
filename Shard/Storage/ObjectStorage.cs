using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Shard.Storage
{
    class ObjectStorage
    {
        #region Properties

        public string InfoLocation { get; private set; }
        public string ObjectsLocation { get; private set; }

        public IEnumerable<PackFile> PackFiles
        {
            get
            {
                //TODO we should be holding on to these pack file instances
                var packs = new DirectoryInfo(this.PacksLocation);
                if (packs.Exists)
                    return packs
                        .EnumerateFiles("*.pack")
                        .Select(pf => new PackFile(pf.FullName));

                return new PackFile[] {};
            }
        }

        public string PacksLocation { get; private set; }

        #endregion

        #region Constructors

        public ObjectStorage(string gitLocation)
        {
            this.ObjectsLocation = Path.Combine(gitLocation, "objects");
            this.PacksLocation = Path.Combine(this.ObjectsLocation, "pack");
            this.InfoLocation = Path.Combine(this.ObjectsLocation, "info");

            //.git/objects
            if(!Directory.Exists(this.ObjectsLocation))
                Directory.CreateDirectory(this.ObjectsLocation);

            //.git/objects/pack
            if (!Directory.Exists(this.PacksLocation))
                Directory.CreateDirectory(this.PacksLocation);

            //.git/objects/info
            if (!Directory.Exists(this.InfoLocation))
                Directory.CreateDirectory(this.InfoLocation);
        }

        #endregion

        #region readers

        public AbstractObject Read(string id)
        {
            var reader = CreateReader(id);
            if (reader == null)
                return null;

            switch (reader.Type)
            {
                case ObjectType.Blob:
                    return CreateBlob(id, reader);
                case ObjectType.Tree:
                case ObjectType.Commit:
                case ObjectType.Tag:
                case ObjectType.OffsetDelta:
                case ObjectType.ReferenceDelta:
                default:
                    throw new NotSupportedException(
                        string.Format("Object Type ({0}) for object ({1}) not supported at this time.", reader.Type, id));
            }
        }

        private static Blob CreateBlob(string id, ObjectReader reader)
        {
            return new Blob(id, reader.Size, LoadDataFromReader(reader));
        }

        private static Func<byte[]> LoadDataFromReader(ObjectReader reader)
        {
            return () =>
            {
                var bytes = new byte[reader.Size];
                reader.Load(stream => stream.Read(bytes, 0, bytes.Length));
                return bytes;
            };
        }

        private ObjectReader CreateReader(string id)
        {
            var loader = LooseObjectReader.GetObjectLoader(this.ObjectsLocation, id);
            if (loader != null)
                return loader;

            var pf = this.PackFiles.FirstOrDefault(pack => pack.HasEntry(id));
            if (pf != null)
                return pf.GetObjectLoader(id);

            return null;
        }

        private FileInfo FileInfoFor(string id)
        {
            return new FileInfo(Path.Combine(this.ObjectsLocation, id.Substring(0, 2), id.Substring(2)));
        }

        private bool InsertUnpacked(FileInfo tmpFile, string id)
        {
            var dest = FileInfoFor(id);
            if (dest.Exists)
            {
                tmpFile.Delete();
                return true;
            }

            if(!dest.Directory.Exists)
                dest.Directory.Create();

            tmpFile.MoveTo(dest.FullName);
            return true;
        }

        private string Insert(byte[] data)
        {
            return Insert(ObjectType.Blob, data);
        }

        private string Insert(ObjectType type, byte[] data)
        {
            string id;
            var file = ObjectWriter.CreateTempFile(type, data.Length, data, this.ObjectsLocation, out id);
            InsertUnpacked(file, id);
            return id;
        }

        #endregion

        #region writers

        public string Write(byte[] data)
        {
            return Insert(data);
        }

        #endregion
    }
}
