using MZBase.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MZBase.Infrastructure
{
    public interface IBaseRepositoryAsync<ModelItem, DBModelEntity, T>
              where ModelItem : Model<T>
                where DBModelEntity : ModelItem, IConvertibleDBModelEntity<ModelItem>, new()
              where T : struct

    {

        #region single retrieve
        /// <summary>
        /// Get item by identifier
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the item if found; otherwise, null.</returns>
        Task<DBModelEntity?> GetByIdAsync(T id);

        /// <summary>
        /// Get item by identifier
        /// </summary>
        /// <param name="id">The identifier of the item</param>
        /// <returns>The result contains the item if found; otherwise, null.</returns>
        DBModelEntity GetById(T id);

        /// <summary>
        /// Get first item that match the predicate
        /// </summary>
        /// <param name="predicate">The predicate to filter items</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the first item that matches the predicate if found; otherwise, null.</returns>
        Task<DBModelEntity?> FirstOrDefaultAsync(Expression<Func<DBModelEntity, bool>> predicate);

        /// <summary>
        /// Get first item that match the predicate
        /// </summary>
        /// <param name="predicate">The predicate to filter items</param>
        /// <returns>The result contains the item if found; otherwise, null.</returns>
        DBModelEntity? FirstOrDefault(Expression<Func<DBModelEntity, bool>> predicate);

        #endregion

        #region group retrieve
        /// <summary>
        /// Get all items
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a read-only list of all items.</returns>
        Task<IReadOnlyList<ModelItem>> AllItemsAsync();

        /// <summary>
        /// Get all items
        /// </summary>
        /// The result contains a read-only list of all items.</returns>
        IReadOnlyList<ModelItem> AllItems();
        #endregion group retrieve

        /// <summary>
        /// Insert a new item
        /// </summary>
        /// <param name="item">The item to insert</param>
        /// <returns>The inserted item</returns>
        DBModelEntity Insert(ModelItem item);

        /// <summary>
        /// Delete an item
        /// </summary>
        /// <param name="item">The item to delete</param>
        void Delete(DBModelEntity item);

        /// <summary>
        /// Update an item
        /// </summary>
        /// <param name="item">The item to update</param>
        void Update(DBModelEntity item);

    }
}
