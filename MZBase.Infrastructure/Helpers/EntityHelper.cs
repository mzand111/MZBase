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
        /// <typeparam name="TDomainModel">The type of the domain model.</typeparam>
        /// <typeparam name="TEntity">The type of the entity model, which must implement IConvertibleDBModelEntity.</typeparam>
        /// <typeparam name="TPrimaryKeyType">The type of the primary key, which must be a value type.</typeparam>
        /// <param name="entityCollection">The collection of entity models to be synchronized.</param>
        /// <param name="domainModelCollection">The collection of domain models to synchronize from.</param>
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
                entityCollection = new List<TEntity>();
            }

            if (domainModelCollection != null)
            {
                var domainModelIds = domainModelCollection.Select(dm => dm.ID).ToList();
                var entitiesToRemove = entityCollection.Where(e => !domainModelIds.Contains(e.ID)).ToList();

                foreach (var entityToRemove in entitiesToRemove)
                {
                    entityCollection.Remove(entityToRemove);
                }

                foreach (var domainModel in domainModelCollection)
                {
                    var existingEntity = entityCollection.FirstOrDefault(e => e.ID.Equals(domainModel.ID));
                    if (existingEntity == null)
                    {
                        var newEntity = new TEntity();
                        newEntity.SetFieldsFromDomainModel(domainModel);
                        entityCollection.Add(newEntity);
                    }
                    else
                    {
                        existingEntity.SetFieldsFromDomainModel(domainModel);
                    }
                }
            }
        }
    }
}
