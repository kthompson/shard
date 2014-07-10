using System;
using System.Linq;
using Xunit;

namespace Shard.Tests
{
    public class SessionTestsWithIntId
    {

        [Fact]
        public void SessionLoadsMultipleObjects()
        {
            IDocumentStore store;
            using (TestHelper.GetDocumentStore(out store))
            {

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
                    var entities = session.Load<BasicObjectWithIntegerId>(1, 2, 3);
                    Assert.Equal(3, entities.Length);

                    foreach (var entity in entities)
                    {
                        Assert.NotNull(entity);
                        Assert.Equal("Object" + entity.Id, entity.Value);
                    }
                }
            }
        }

        [Fact]
        public void SavingSessionSavesChangedObjects()
        {
            var value = DateTime.Now.ToString();
            IDocumentStore store;
            using (TestHelper.GetDocumentStore(out store))
            {
                const long id = 123456L;

                using (var session = store.OpenSession())
                {
                    session.Store(new BasicObjectWithIntegerId
                    {
                        Id = id,
                        Value = "hi",
                    });
                    session.SaveChanges(); // will send the change to the database
                }

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
        }

        [Fact]
        public void SessionSavesAndLoadsData()
        {
            IDocumentStore store;
            using (TestHelper.GetDocumentStore(out store))
            {
                using (var session = store.OpenSession())
                {
                    session.Store(new BasicObjectWithIntegerId {
                        Id = 123456,
                        Value = "Some Value"
                    });
                    session.SaveChanges();
                }

                using (var session = store.OpenSession())
                {
                    var entity = session.Load<BasicObjectWithIntegerId>(123456);
                    Assert.NotNull(entity);
                    Assert.Equal("Some Value", entity.Value);
                }
            }
        }


        [Fact]
        public void SessionSetsIdOnSave()
        {
            IDocumentStore store;
            using (TestHelper.GetDocumentStore(out store))
            using (var session = store.OpenSession())
            {
                var entity = new BasicObjectWithIntegerId {Value = "Some Value"};
                session.Store(entity);
                session.SaveChanges();

                Assert.NotEqual(0, entity.Id);
            }
        }

        [Fact]
        public void SessionDoesntSaveUnlessSaveChangesIsCalled()
        {
            const long id = 123465;

            IDocumentStore store;
            using (TestHelper.GetDocumentStore(out store))
            {
                using (var session = store.OpenSession())
                {
                    session.Store(new BasicObjectWithIntegerId
                    {
                        Id = id,
                        Value = "Some Value"
                    });
                }

                using (var session = store.OpenSession())
                {
                    var entity = session.Load<BasicObjectWithIntegerId>(id);
                    Assert.Null(entity);
                }
            }
        }

        [Fact]
        public void SessionCanCreateObjectsWithoutIdCollisions()
        {
            IDocumentStore store;
            using (TestHelper.GetDocumentStore(out store))
            {
                using (var session = store.OpenSession())
                {
                    for (int i = 0; i < 100; i++)
                    {
                        session.Store(new BasicObjectWithIntegerId
                        {
                            Value = "Some Value" + i
                        });    
                    }
                    
                    session.SaveChanges();
                }

                // an id collision would most likely have duplicate ids
                using (var session = store.OpenSession())
                {
                    var items = session.LoadAll<BasicObjectWithIntegerId>();

                    Assert.Equal(100, items.Count());
                }
            }
        }
    }
}