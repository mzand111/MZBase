using MZBase.Domain;
using System.Threading.Tasks;

namespace MZBase.Infrastructure
{
    public interface IRepositoryAsync<ModelItem, DBModelEntity, T> : IBaseRepositoryAsync<ModelItem, DBModelEntity, T>
          where ModelItem : Model<T>
         where DBModelEntity : ModelItem, IConvertibleDBModelEntity<ModelItem>, new()
          where T : struct
    {
        void SaveChanges();
        Task SaveChangesAsync();
    }
}
