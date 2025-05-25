using MZBase.Domain;

namespace MZBase.Infrastructure
{
    public interface IDynamicTestableUnitOfWorkAsync : IUnitOfWorkAsync
    {
        IBaseLDRCompatibleRepositoryAsync<DomainModel, DBModelEntity, PrimKey> GetRepo<DomainModel, DBModelEntity, PrimKey>()
            where DomainModel : Model<PrimKey>
             where DBModelEntity : DomainModel, IConvertibleDBModelEntity<DomainModel>, new()
           where PrimKey : struct;
    }
}
