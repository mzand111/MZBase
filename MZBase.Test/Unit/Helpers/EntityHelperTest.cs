using MZBase.Domain;
using MZBase.Infrastructure;
using MZBase.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MZBase.Test.Unit.Helpers
{
    public class EntityHelperTest
    {
        public class SampleChild : Model<int>
        {
            public string Value { get; set; }
        }

        public class SampleChildEntity : SampleChild, IConvertibleDBModelEntity<SampleChild>
        {
            public SampleChild GetDomainObject() => this;

            public void SetFieldsFromDomainModel(SampleChild domainModelEntity)
            {
                ID = domainModelEntity.ID;
                Value = domainModelEntity.Value;
            }
        }

        public class SampleGuidChild : Model<Guid>
        {
            public string Value { get; set; }
        }

        public class SampleGuidChildEntity : SampleGuidChild, IConvertibleDBModelEntity<SampleGuidChild>
        {
            public SampleGuidChild GetDomainObject() => this;

            public void SetFieldsFromDomainModel(SampleGuidChild domainModelEntity)
            {
                ID = domainModelEntity.ID;
                Value = domainModelEntity.Value;
            }
        }

        [Fact]
        public void Sync_CreatesOneEntityPerNewDomainModelWithDefaultKey()
        {
            // Regression test: previously every new (ID == 0) domain model matched the entity
            // created for the first one, collapsing the collection into a single entity holding
            // the last submitted item's values.
            var domainCollection = new List<SampleChild>
            {
                new SampleChild { Value = "A" },
                new SampleChild { Value = "B" },
                new SampleChild { Value = "C" }
            };
            var entityCollection = new List<SampleChildEntity>();

            EntityHelper.SyncDomainAndEntityModelSubCollections<SampleChild, SampleChildEntity, int>(
                domainCollection, entityCollection);

            Assert.Equal(3, entityCollection.Count);
            Assert.Equal(new[] { "A", "B", "C" }, entityCollection.Select(e => e.Value).ToArray());
            Assert.All(entityCollection, e => Assert.Equal(0, e.ID));
        }

        [Fact]
        public void Sync_CreatesOneEntityPerNewDomainModelWithDefaultGuidKey()
        {
            var domainCollection = new List<SampleGuidChild>
            {
                new SampleGuidChild { Value = "A" },
                new SampleGuidChild { Value = "B" },
                new SampleGuidChild { Value = "C" }
            };
            var entityCollection = new List<SampleGuidChildEntity>();

            EntityHelper.SyncDomainAndEntityModelSubCollections<SampleGuidChild, SampleGuidChildEntity, Guid>(
                domainCollection, entityCollection);

            Assert.Equal(3, entityCollection.Count);
            Assert.Equal(new[] { "A", "B", "C" }, entityCollection.Select(e => e.Value).ToArray());
        }

        [Fact]
        public void Sync_UpdatesTrackedChildrenByRealKey()
        {
            var entityCollection = new List<SampleChildEntity>
            {
                new SampleChildEntity { ID = 10, Value = "old-10" },
                new SampleChildEntity { ID = 11, Value = "old-11" }
            };
            var domainCollection = new List<SampleChild>
            {
                new SampleChild { ID = 10, Value = "new-10" },
                new SampleChild { ID = 11, Value = "new-11" }
            };

            EntityHelper.SyncDomainAndEntityModelSubCollections<SampleChild, SampleChildEntity, int>(
                domainCollection, entityCollection);

            Assert.Equal(2, entityCollection.Count);
            Assert.Equal("new-10", entityCollection.First(e => e.ID == 10).Value);
            Assert.Equal("new-11", entityCollection.First(e => e.ID == 11).Value);
        }

        [Fact]
        public void Sync_RemovesTrackedChildrenMissingFromSubmission()
        {
            var entityCollection = new List<SampleChildEntity>
            {
                new SampleChildEntity { ID = 10, Value = "kept" },
                new SampleChildEntity { ID = 12, Value = "removed" }
            };
            var domainCollection = new List<SampleChild>
            {
                new SampleChild { ID = 10, Value = "kept" }
            };

            EntityHelper.SyncDomainAndEntityModelSubCollections<SampleChild, SampleChildEntity, int>(
                domainCollection, entityCollection);

            Assert.Single(entityCollection);
            Assert.Equal(10, entityCollection.Single().ID);
        }

        [Fact]
        public void Sync_MixesUpdatesAndNewItemsInOneCall()
        {
            var entityCollection = new List<SampleChildEntity>
            {
                new SampleChildEntity { ID = 10, Value = "old" }
            };
            var domainCollection = new List<SampleChild>
            {
                new SampleChild { ID = 10, Value = "updated" },
                new SampleChild { Value = "created-1" },
                new SampleChild { Value = "created-2" }
            };

            EntityHelper.SyncDomainAndEntityModelSubCollections<SampleChild, SampleChildEntity, int>(
                domainCollection, entityCollection);

            Assert.Equal(3, entityCollection.Count);
            Assert.Equal("updated", entityCollection.First(e => e.ID == 10).Value);
            Assert.Equal(2, entityCollection.Count(e => e.ID == 0));
            Assert.Equal(new[] { "created-1", "created-2" }, entityCollection.Where(e => e.ID == 0).Select(e => e.Value).ToArray());
        }

        [Fact]
        public void Sync_RepeatedCallPairsUnsavedItemsByOrderWithoutDuplicating()
        {
            var entityCollection = new List<SampleChildEntity>();
            var firstSubmission = new List<SampleChild>
            {
                new SampleChild { Value = "A" },
                new SampleChild { Value = "B" }
            };

            EntityHelper.SyncDomainAndEntityModelSubCollections<SampleChild, SampleChildEntity, int>(
                firstSubmission, entityCollection);

            var secondSubmission = new List<SampleChild>
            {
                new SampleChild { Value = "A2" },
                new SampleChild { Value = "B2" },
                new SampleChild { Value = "C2" }
            };

            EntityHelper.SyncDomainAndEntityModelSubCollections<SampleChild, SampleChildEntity, int>(
                secondSubmission, entityCollection);

            Assert.Equal(3, entityCollection.Count);
            Assert.Equal(new[] { "A2", "B2", "C2" }, entityCollection.Select(e => e.Value).ToArray());
        }

        [Fact]
        public void Sync_NullDomainCollectionLeavesEntityCollectionUnchanged()
        {
            var entityCollection = new List<SampleChildEntity>
            {
                new SampleChildEntity { ID = 10, Value = "kept" }
            };

            EntityHelper.SyncDomainAndEntityModelSubCollections<SampleChild, SampleChildEntity, int>(
                null, entityCollection);

            Assert.Single(entityCollection);
            Assert.Equal("kept", entityCollection.Single().Value);
        }

        [Fact]
        public void Sync_NullEntityCollectionThrows()
        {
            var domainCollection = new List<SampleChild> { new SampleChild { Value = "A" } };

            Assert.Throws<ArgumentNullException>(() =>
                EntityHelper.SyncDomainAndEntityModelSubCollections<SampleChild, SampleChildEntity, int>(
                    domainCollection, null));
        }
    }
}
