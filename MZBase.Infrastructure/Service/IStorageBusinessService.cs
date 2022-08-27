using MZBase.Domain;
using MZSimpleDynamicLinq.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MZBase.Infrastructure.Service
{
    public interface IStorageBusinessService<T, PrimarykeyType>
        where T : IModel<PrimarykeyType>
        where PrimarykeyType : struct
    {


        public int LogBaseID { get; }

        Task<PrimarykeyType> AddAsync(T item);
        Task<T> RetrieveByIdAsync(PrimarykeyType ID);
        Task ModifyAsync(T item);
        Task RemoveByIdAsync(PrimarykeyType ID);
        Task<LinqDataResult<T>> ItemsAsync(LinqDataRequest request);
    }
}
