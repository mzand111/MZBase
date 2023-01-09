using MZBase.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MZBase.Infrastructure
{
    /// <summary>
    /// Pageable, sortable and filtarable reop interface
    /// </summary>
    /// <typeparam name="ModelItem"></typeparam>
    /// <typeparam name="T"></typeparam>
    public interface IPSFRepositoryAsync <ModelItem, T>: IRepositoryAsync<ModelItem, T>
          where ModelItem : Model<T>
          where T : struct
    {
        Task<IReadOnlyList<ModelItem>> AllItemsAsync(int pageSize, int skip);
        Task<IReadOnlyList<ModelItem>> AllItemsAsync(int pageSize, int skip, string sortColumn, string sortDirection);
        Task<int> GetTotalCountAsync();
    }
}
