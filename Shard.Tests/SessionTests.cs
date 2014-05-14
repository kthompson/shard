using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Shard;

namespace Shard.Tests
{
    public class SessionTests : SessionTestsBase
    {
        [Fact]
        public void SessionIsQueryable()
        {
            var store = GetTestDocumentStore();

            using (var session = store.OpenSession())
            {
                session.Store(new BasicObject
                {
                    Id = "basicobjects/1",
                    Value = "1"
                });

                session.Store(new BasicObject
                {
                    Id = "basicobjects/2",
                    Value = "2"
                });

                session.Store(new BasicObject
                {
                    Id = "basicobjects/3",
                    Value = "3"
                });

                session.SaveChanges();
            }

            using (var session = store.OpenSession())
            {
                var results = from entity in session.Query<BasicObject>()
                              where int.Parse(entity.Value) > 1
                              select entity;

                var entities = results.ToList();

                Assert.Equal(2, entities.Count);

                foreach (var entity in entities)
                    Assert.Equal("basicobjects/" + entity.Value, entity.Id);
            }
        }

        [Fact]
        public void SessionCanDeleteObjects()
        {
            var store = GetTestDocumentStore();

            using (var session = store.OpenSession())
            {
                session.Store(new BasicObject
                {
                    Id = "basicobjects/1",
                    Value = "1"
                });

                session.SaveChanges();
            }

            using (var session = store.OpenSession())
            {
                var entity = session.Load<BasicObject>("basicobjects/1");
                Assert.NotNull(entity);
                session.Delete(entity);
                session.SaveChanges();
            }

            using (var session = store.OpenSession())
            {
                var entity = session.Load<BasicObject>("basicobjects/1");
                Assert.Null(entity);
            }

        }


        [Fact]
        public void SessionLoadsMultipleObjects()
        {
            var store = GetTestDocumentStore();

            using (var session = store.OpenSession())
            {
                session.Store(new BasicObject
                {
                    Id = "basicobjects/1",
                    Value = "1"
                });

                session.Store(new BasicObject
                {
                    Id = "basicobjects/2",
                    Value = "2"
                });

                session.Store(new BasicObject
                {
                    Id = "basicobjects/3",
                    Value = "3"
                });

                session.SaveChanges();
            }

            using (var session = store.OpenSession())
            {
                var entities = session.Load<BasicObject>("basicobjects/1", "basicobjects/2", "basicobjects/3");
                Assert.Equal(3, entities.Length);

                foreach (var entity in entities)
                    Assert.Equal("basicobjects/" + entity.Value, entity.Id);
            }
        }

        [Fact]
        public void SavingSessionSavesChangedObjects()
        {
            var value = DateTime.Now.ToString();
            var store = GetTestDocumentStore();
            using (var session = store.OpenSession())
            {
                var entity = session.Load<BasicObject>("abc123");
                Assert.NotEqual(value, entity.Value);
                entity.Value = value;
                session.SaveChanges(); // will send the change to the database
            }

            using (var session = store.OpenSession())
            {
                var entity = session.Load<BasicObject>("abc123");
                Assert.Equal(value, entity.Value);
            }
        }

        [Fact]
        public void SessionSavesAndLoadsData()
        {
            string id;
            var documentStore = GetTestDocumentStore();
            using (var session = documentStore.OpenSession())
            {
                var entity = new BasicObject { Value = "Some Value" };
                session.Store(entity);
                session.SaveChanges();
                id = entity.Id;
                Assert.NotNull(id);
            }

            using (var session = documentStore.OpenSession())
            {
                var entity = session.Load<BasicObject>(id);
                Assert.Equal("Some Value", entity.Value);
            }
        }

        [Fact]
        public void SessionDoesntSaveUnlessSaveChangesIsCalled()
        {
            const string id = "abc123";

            var documentStore = GetTestDocumentStore();
            using (var session = documentStore.OpenSession())
            {
                session.Store(new BasicObject
                {
                    Id = id,
                    Value = "Some Value"
                });
            }

            using (var session = documentStore.OpenSession())
            {
                var entity = session.Load<BasicObject>(id);
                Assert.Null(entity);
            }
        }
    }
}
