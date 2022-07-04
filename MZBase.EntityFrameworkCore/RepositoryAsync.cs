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
    public class RepositoryAsync<ModelEntity, PrimaryKeyType> : IRepositoryAsync<ModelEntity, PrimaryKeyType>
        where ModelEntity : Model<PrimaryKeyType>
        where PrimaryKeyType : struct
    {
        protected readonly DbSet<ModelEntity> _entities;
        protected readonly DbContext _context;
        public RepositoryAsync(DbContext context)
        {
            _entities = context.Set<ModelEntity>();
            _context = context;
        }
        public async Task<IReadOnlyList<ModelEntity>> AllItemsAsync() => await _entities.ToListAsync();
        public Task<ModelEntity> FirstOrDefaultAsync(Expression<Func<ModelEntity, bool>> predicate) => _entities.FirstOrDefaultAsync(predicate);
        public async Task DeleteAsync(ModelEntity item) => _entities.Remove(item);
        public async Task<ModelEntity> InsertAsync(ModelEntity item) => _entities.Add(item).Entity;

        public async Task<ModelEntity> GetByIdAsync(PrimaryKeyType id) => await _entities.FindAsync(id);

        public async Task UpdateAsync(ModelEntity item)
        {
            _context.Entry(item).State = EntityState.Modified;
            return;
        }
    }
}
