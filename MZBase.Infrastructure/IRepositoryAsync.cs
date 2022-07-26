using MZBase.Domain;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MZBase.Infrastructure
{
    public interface IRepositoryAsync<ModelItem, T>
          where ModelItem : Model<T>
          where T : struct

    {

        #region single retrieve
        /// <summary>
        /// Get item by identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ModelItem?> GetByIdAsync(T id);
        Task<ModelItem?> FirstOrDefaultAsync(Expression<Func<ModelItem, bool>> predicate);
        #endregion

        #region group retrieve
        Task<IReadOnlyList<ModelItem>> AllItemsAsync();    
        #endregion


        


        Task<ModelItem> InsertAsync(ModelItem item);
        Task DeleteAsync(ModelItem item);
        Task UpdateAsync(ModelItem item);

    }
}
