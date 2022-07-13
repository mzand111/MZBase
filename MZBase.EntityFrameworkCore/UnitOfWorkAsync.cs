using Microsoft.EntityFrameworkCore;
using MZBase.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MZBase.EntityFrameworkCore
{
    public class UnitOfWorkAsync<T> : IUnitOfWorkAsync
        where T : DbContext
    {
        protected readonly T _dbContext;
        private bool disposed;

        public UnitOfWorkAsync(T dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<int> CommitAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
            }
            disposed = true;
        }
    }
}
