using MZBase.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MZBase.Infrastructure.Helpers
{
    public static class EntityHelper
    {

        /// <summary>
        /// Synchronizes a collection of entity models with a collection of domain models.
        /// This method is designed to ensure that the entity collection reflects the state of the domain model collection.
        /// This method ensures that:
        /// 1. Entities not present in the domain model collection are removed from the entity collection.
        /// 2. New entities are created and added for domain models not present in the entity collection.
        /// 3. Existing entities are updated with the values from the corresponding domain models.
        ///
        /// Domain models whose <see cref="Model{TPrimaryKeyType}.ID"/> equals the default value of
        /// <typeparamref name="TPrimaryKeyType"/> (for example 0 for numeric identity keys, or
        /// <see cref="System.Guid.Empty"/> for Guid keys) are treated as "not yet persisted" items:
        /// each of them produces its own new entity instead of being matched against other
        /// unsaved entities. This matters when several new items are submitted in one call: the
        /// previous implementation matched unsaved entities by their (all default) key and every
        /// new domain model after the first one overwrote the entity created for the first one,
        /// silently collapsing the whole collection into a single item.
        ///
        /// When an entity collection that already contains unsaved entities (created by an earlier
        /// call on the same entity instance) is synchronized again, the unsaved entities and the
        /// unsaved domain models are paired by their order of appearance, so a repeated call
        /// updates them in place instead of duplicating them. Surplus items on either side are
        /// removed or added accordingly.
        ///
        /// A null <paramref name="domainModelCollection"/> is treated as "no change" and leaves the
        /// entity collection untouched.
        ///
        /// <typeparam name="TDomainModel">The type of the domain model.</typeparam>
        /// <typeparam name="TEntity">The type of the entity model, which must implement IConvertibleDBModelEntity.</typeparam>
        /// <typeparam name="TPrimaryKeyType">The type of the primary key, which must be a value type.</typeparam>
        /// <param name="domainModelCollection">The collection of domain models to synchronize from.</param>
        /// <param name="entityCollection">The collection of entity models to be synchronized.</param>
        /// </summary>
        public static void SyncDomainAndEntityModelSubCollections<TDomainModel, TEntity, TPrimaryKeyType>(

            ICollection<TDomainModel> domainModelCollection,
             ICollection<TEntity> entityCollection)
            where TEntity : TDomainModel, IConvertibleDBModelEntity<TDomainModel>, new()
            where TDomainModel : Model<TPrimaryKeyType>
            where TPrimaryKeyType : struct
        {
            if (entityCollection == null)
            {
                throw new ArgumentNullException(nameof(entityCollection));
            }

            if (domainModelCollection == null)
            {
                return;
            }

            var submittedRealKeys = domainModelCollection
                .Where(dm => !IsDefaultKey(dm.ID))
                .Select(dm => dm.ID)
                .ToList();

            var entitiesToRemove = entityCollection
                .Where(e => !IsDefaultKey(e.ID) && !submittedRealKeys.Contains(e.ID))
                .ToList();

            foreach (var entityToRemove in entitiesToRemove)
            {
                entityCollection.Remove(entityToRemove);
            }

            var unsavedEntities = entityCollection.Where(e => IsDefaultKey(e.ID)).ToList();
            var unsavedDomainModels = domainModelCollection.Where(dm => IsDefaultKey(dm.ID)).ToList();

            var pairedCount = Math.Min(unsavedEntities.Count, unsavedDomainModels.Count);
            for (var i = 0; i < pairedCount; i++)
            {
                unsavedEntities[i].SetFieldsFromDomainModel(unsavedDomainModels[i]);
            }

            for (var i = pairedCount; i < unsavedEntities.Count; i++)
            {
                entityCollection.Remove(unsavedEntities[i]);
            }

            for (var i = pairedCount; i < unsavedDomainModels.Count; i++)
            {
                var newEntity = new TEntity();
                newEntity.SetFieldsFromDomainModel(unsavedDomainModels[i]);
                entityCollection.Add(newEntity);
            }

            var trackedEntitiesByRealKey = entityCollection
                .Where(e => !IsDefaultKey(e.ID))
                .GroupBy(e => e.ID)
                .ToDictionary(group => group.Key, group => group.First());

            foreach (var domainModel in domainModelCollection.Where(dm => !IsDefaultKey(dm.ID)))
            {
                if (trackedEntitiesByRealKey.TryGetValue(domainModel.ID, out var existingEntity))
                {
                    existingEntity.SetFieldsFromDomainModel(domainModel);
                }
                else
                {
                    var newEntity = new TEntity();
                    newEntity.SetFieldsFromDomainModel(domainModel);
                    entityCollection.Add(newEntity);
                }
            }
        }

        private static bool IsDefaultKey<TPrimaryKeyType>(TPrimaryKeyType key)
            where TPrimaryKeyType : struct
        {
            // Note: `key.Equals(default)` must not be used here. On an unconstrained type
            // parameter, `Equals` resolves to object.Equals(object), so the bare `default`
            // literal is converted to null and the check would always return false.
            return EqualityComparer<TPrimaryKeyType>.Default.Equals(key, default);
        }
    }
}
