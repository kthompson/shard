using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shard.Tests
{
    public class Benchmarks : IDisposable
    {
        private readonly IDisposable _cleanupStoreToken;
        readonly IDocumentStore _store;

        public Benchmarks()
        {
            _cleanupStoreToken = TestHelper.GetDocumentStore(out _store);

            using (var session = _store.OpenSession())
            {
                session.Store(new BasicObject
                {
                    Id = "basicobject/1",
                    Value = Guid.NewGuid().ToString("B")
                });

                session.Store(new BasicObject
                {
                    Id = "basicobject/2",
                    Value = Guid.NewGuid().ToString("B")
                });

                session.Store(new BasicObject
                {
                    Id = "basicobject/3",
                    Value = Guid.NewGuid().ToString("B")
                });

                session.SaveChanges();
            }
        }

        [Benchmark]
        public void StoringDocumentsShouldBeFast()
        {
            using (var session = _store.OpenSession())
            {
                session.Store(new BasicObject
                {
                    Value = Guid.NewGuid().ToString("B")
                });

                session.SaveChanges();
            }
        }


        [Benchmark]
        public void LoadingDocumentsShouldBeFast()
        {
            using (var session = _store.OpenSession())
            {
                var items = session.Load<BasicObject>(1, 2, 3);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cleanupStoreToken.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
