using MZBase.Domain;
using MZSimpleDynamicLinq.Core;
using System.Threading.Tasks;

namespace MZBase.Infrastructure
{
    /// <summary>
    /// Repository interface compatible with handling LinqDataRequest
    /// </summary>
    /// <typeparam name="ModelItem"></typeparam>
    /// <typeparam name="T"></typeparam>
    public interface ILDRCompatibleRepositoryAsync<ModelItem, DBModelEntity, T> : IBaseRepositoryAsync<ModelItem, DBModelEntity, T>
          where ModelItem : Model<T>
         where DBModelEntity : ModelItem, IConvertibleDBModelEntity<ModelItem>, new()
          where T : struct
    {
        Task<LinqDataResult<ModelItem>> AllItemsAsync(LinqDataRequest request);
    }
}
