﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Shard;

namespace Shard.Tests
{
    public class SessionTests
    {
        [Fact(Skip = "Not yet implemented")]
        public void SessionIsQueryable()
        {
            IDocumentStore store;
            using (TestHelper.GetDocumentStore(out store))
            {
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
        }

        [Fact]
        public void SessionCanDeleteObjects()
        {
            IDocumentStore store;
            using (TestHelper.GetDocumentStore(out store))
            {
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
        }

        [Fact]
        public void SessionLoadsMultipleObjects()
        {
            IDocumentStore store;
            using (TestHelper.GetDocumentStore(out store))
            {
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
        }

        [Fact]
        public void SavingSessionSavesChangedObjects()
        {
            IDocumentStore store;
            using (TestHelper.GetDocumentStore(out store))
            {
                var value = DateTime.Now.ToString();

                using (var session = store.OpenSession())
                {
                    session.Store(new BasicObject
                    {
                        Id = "abc123",
                        Value = "1"
                    });

                    session.SaveChanges();
                }

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
        }
        
        [Fact]
        public void SessionSaveChangesSetsId()
        {
            IDocumentStore store;
            using (TestHelper.GetDocumentStore(out store))
            {
                using (var session = store.OpenSession())
                {
                    var entity = new BasicObject {Value = "Some Value"};
                    Assert.Null(entity.Id);
                    session.Store(entity);

                    session.SaveChanges();
                    Assert.NotNull(entity.Id);
                }
            }
        }

        [Fact]
        public void SessionSavesAndLoadsData()
        {
            IDocumentStore store;
            using (TestHelper.GetDocumentStore(out store))
            {
                const string id = "basicobject/123";
                using (var session = store.OpenSession())
                {
                    var entity = new BasicObject
                    {
                        Id = id,
                        Value = "Some Value"
                    };
                    session.Store(entity);
                    session.SaveChanges();
                    
                    Assert.Equal(id, entity.Id);
                }

                using (var session = store.OpenSession())
                {
                    var entity = session.Load<BasicObject>(id);
                    Assert.Equal(id, entity.Id);
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
                var entity = new BasicObject { Value = "Some Value" };
                session.Store(entity);
                session.SaveChanges();

                Assert.NotEqual("", entity.Id);
            }
        }

        [Fact]
        public void SessionDoesntSaveUnlessSaveChangesIsCalled()
        {
            const string id = "abc123";

            IDocumentStore store;
            using (TestHelper.GetDocumentStore(out store))
            {
                using (var session = store.OpenSession())
                {
                    session.Store(new BasicObject
                    {
                        Id = id,
                        Value = "Some Value"
                    });
                }

                using (var session = store.OpenSession())
                {
                    var entity = session.Load<BasicObject>(id);
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
                        session.Store(new BasicObject
                        {
                            Value = "Some Value" + i
                        });
                    }

                    session.SaveChanges();
                }

                // an id collision would most likely have duplicate ids
                using (var session = store.OpenSession())
                {
                    var items = session.LoadAll<BasicObject>();

                    Assert.Equal(100, items.Count());
                }
            }
        }
    }
}
