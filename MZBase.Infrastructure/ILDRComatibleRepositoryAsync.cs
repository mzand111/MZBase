using MZBase.Domain;
using MZSimpleDynamicLinq.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MZBase.Infrastructure
{
    /// <summary>
    /// Ripository interface compatible with handling LinqDataRequest
    /// </summary>
    /// <typeparam name="ModelItem"></typeparam>
    /// <typeparam name="T"></typeparam>
    public interface ILDRCompatibleRepositoryAsync<ModelItem, T>: IRepositoryAsync<ModelItem, T>
          where ModelItem : Model<T>
          where T : struct
    {
        Task<LinqDataResult<ModelItem>> AllItemsAsync(LinqDataRequest request);
    }
}
