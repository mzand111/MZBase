using Microsoft.EntityFrameworkCore;
using MZBase.Domain;
using MZBase.Infrastructure;

namespace MZBase.EntityFrameworkCore
{
    public abstract class RepositoryAsync<DomainModelEntity, DBModelEntity, PrimaryKeyType> : BaseRepositoryAsync<DomainModelEntity, DBModelEntity, PrimaryKeyType>, IRepositoryAsync<DomainModelEntity, DBModelEntity, PrimaryKeyType>
            where DomainModelEntity : Model<PrimaryKeyType>
            where DBModelEntity : DomainModelEntity, IConvertibleDBModelEntity<DomainModelEntity>, new()
            where PrimaryKeyType : struct
    {
        protected RepositoryAsync(DbContext context) : base(context)
        {
        }

        public void SaveChanges() => _context.SaveChanges();

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
