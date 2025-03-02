using Microsoft.EntityFrameworkCore;
using MZBase.Domain;
using MZBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MZBase.EntityFrameworkCore
{
    public abstract class RepositoryAsync<DBModelEntity,DomainModelEntity, PrimaryKeyType> : IRepositoryAsync<DomainModelEntity, PrimaryKeyType>
        where DomainModelEntity : Model<PrimaryKeyType>
        where DBModelEntity: DomainModelEntity
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
        public virtual Task<DomainModelEntity?> FirstOrDefaultAsync(Expression<Func<DomainModelEntity, bool>> predicate) => _entities.FirstOrDefaultAsync(predicate);
        public virtual async Task DeleteAsync(DomainModelEntity item)
        {
            if (item is DBModelEntity)
            {
                _entities.Remove(item as DBModelEntity);
            }
            
        }
        public virtual DomainModelEntity Insert(DomainModelEntity item)
        {
            if (item is DBModelEntity)
            {
                return _entities.Add(item as DBModelEntity).Entity;
            }
            else
            {
                return default(DomainModelEntity);
            }
        }

        public virtual async Task<DomainModelEntity?> GetByIdAsync(PrimaryKeyType id) => await _entities.FindAsync(id);

        public virtual async Task UpdateAsync(DomainModelEntity item)
        {
            _context.Entry(item).State = EntityState.Modified;
            return;
        }
    }
}
