using MZBase.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MZBase.Infrastructure
{
    /// <summary>
    /// Page-able, sortable and filterable repo interface
    /// </summary>
    /// <typeparam name="ModelItem"></typeparam>
    /// <typeparam name="T"></typeparam>
    public interface IPSFRepositoryAsync<ModelItem, DBModelEntity, T> : IRepositoryAsync<ModelItem, DBModelEntity, T>
          where ModelItem : Model<T>
         where DBModelEntity : ModelItem, IConvertibleDBModelEntity<ModelItem>, new()
          where T : struct
    {
        Task<IReadOnlyList<ModelItem>> AllItemsAsync(int pageSize, int skip);
        Task<IReadOnlyList<ModelItem>> AllItemsAsync(int pageSize, int skip, string sortColumn, string sortDirection);
        Task<int> GetTotalCountAsync();
    }
}
