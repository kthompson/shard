using System;
using System.IO;
using Shard.Storage;

namespace Shard.Tests
{
    public static class TestHelper
    {
        public static IDisposable GetDocumentStore(out IDocumentStore store)
        {
            var p = Path.Combine(Path.GetTempPath(), "Shard", Guid.NewGuid().ToString("N"));
            store = new EmbeddedDocumentStore
            {
                Path = p
            };

            store.Initialize();

            return new TestDocumentStoreDisposer((EmbeddedDocumentStore)store);
        }

        class TestDocumentStoreDisposer : IDisposable
        {
            private readonly EmbeddedDocumentStore _store;

            public TestDocumentStoreDisposer(EmbeddedDocumentStore store)
            {
                _store = store;
            }

            public void Dispose()
            {
                _store.Dispose();
                Directory.Delete(_store.Path, true);
            }
        }
    }
}