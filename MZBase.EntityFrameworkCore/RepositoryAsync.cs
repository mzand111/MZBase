using Microsoft.EntityFrameworkCore;
using MZBase.Domain;
using MZBase.Infrastructure;
using System.Linq.Expressions;

namespace MZBase.EntityFrameworkCore
{
    public abstract class RepositoryAsync<DomainModelEntity, DBModelEntity, PrimaryKeyType> : IRepositoryAsync<DomainModelEntity, DBModelEntity, PrimaryKeyType>
            where DomainModelEntity : Model<PrimaryKeyType>
            where DBModelEntity : DomainModelEntity, IConvertibleDBModelEntity<DomainModelEntity>, new()
            where PrimaryKeyType : struct
    {
        protected readonly DbSet<DBModelEntity> _entities;
        protected readonly DbContext _context;
        public RepositoryAsync(DbContext context)
        {
            _entities = context.Set<DBModelEntity>();
            _context = context;
        }
        public virtual async Task<IReadOnlyList<DomainModelEntity>> AllItemsAsync() => await _entities.ToListAsync();
        public virtual Task<DBModelEntity?> FirstOrDefaultAsync(Expression<Func<DBModelEntity, bool>> predicate) => _entities.FirstOrDefaultAsync(predicate);
        public virtual async void Delete(DBModelEntity item)
        {
            _entities.Remove(item);
        }
        public virtual DBModelEntity Insert(DomainModelEntity item)
        {
            if (item is DBModelEntity ent)
            {
                return _entities.Add(ent).Entity;
            }
            else
            {
                DBModelEntity dbEntityObject = new DBModelEntity();
                dbEntityObject.SetFieldsFromDomainModel(item);
                return _entities.Add(dbEntityObject).Entity;
            }
        }

        public virtual async Task<DBModelEntity?> GetByIdAsync(PrimaryKeyType id) => await _entities.FindAsync(id);

        public virtual void Update(DBModelEntity item)
        {
            _context.Entry(item).State = EntityState.Modified;
            return;
        }
    }
}
