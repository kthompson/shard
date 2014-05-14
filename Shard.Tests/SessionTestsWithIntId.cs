using System;
using Xunit;

namespace Shard.Tests
{
    public class SessionTestsWithIntId : SessionTestsBase
    {

        [Fact]
        public void SessionLoadsMultipleObjects()
        {
            var store = GetTestDocumentStore();

            using (var session = store.OpenSession())
            {
                session.Store(new BasicObjectWithIntegerId
                {
                    Id = 1,
                    Value = "Object1"
                });

                session.Store(new BasicObjectWithIntegerId
                {
                    Id = 2,
                    Value = "Object2"
                });

                session.Store(new BasicObjectWithIntegerId
                {
                    Id = 3,
                    Value = "Object3"
                });

                session.SaveChanges();
            }

            using (var session = store.OpenSession())
            {
                var entities = session.Load<BasicObjectWithIntegerId>(1,2,3);
                Assert.Equal(3, entities.Length);

                foreach (var entity in entities)
                    Assert.Equal("Object" + entity.Id, entity.Value);
            }
        }

        [Fact]
        public void SavingSessionSavesChangedObjects()
        {
            var value = DateTime.Now.ToString();
            var store = GetTestDocumentStore();
            var id = 123456L;
            using (var session = store.OpenSession())
            {
                var entity = session.Load<BasicObjectWithIntegerId>(id);
                Assert.NotEqual(value, entity.Value);
                entity.Value = value;
                session.SaveChanges(); // will send the change to the database
            }

            using (var session = store.OpenSession())
            {
                var entity = session.Load<BasicObjectWithIntegerId>(id);
                Assert.Equal(value, entity.Value);
            }
        }

        [Fact]
        public void SessionSavesAndLoadsData()
        {
            long id;
            var documentStore = GetTestDocumentStore();
            using (var session = documentStore.OpenSession())
            {
                var entity = new BasicObjectWithIntegerId { Value = "Some Value" };
                session.Store(entity);
                session.SaveChanges();
                id = entity.Id;
                Assert.NotNull(id);
            }

            using (var session = documentStore.OpenSession())
            {
                var entity = session.Load<BasicObjectWithIntegerId>(id);
                Assert.Equal("Some Value", entity.Value);
            }
        }

        [Fact]
        public void SessionDoesntSaveUnlessSaveChangesIsCalled()
        {
            const long id = 123465;

            var documentStore = GetTestDocumentStore();
            using (var session = documentStore.OpenSession())
            {
                session.Store(new BasicObjectWithIntegerId
                {
                    Id = id,
                    Value = "Some Value"
                });
            }

            using (var session = documentStore.OpenSession())
            {
                var entity = session.Load<BasicObjectWithIntegerId>(id);
                Assert.Null(entity);
            }
        }
    }
}