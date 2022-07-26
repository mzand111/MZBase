using MZBase.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MZBase.Infrastructure
{
    public interface IUnitOfWorkAsync : IDisposable
    {
        Task<int> CommitAsync();
    }
}
